using Administration.Mapping.Contact;
using Administration.Option.Options.Data;
using DataLayer;
using DataLayer.SqlData.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using DatabaseSynchronizeFromCsv = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCsv;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DataLayer.SqlData.Account;
using DataLayer.SqlData.Group;
using DataLayer.SqlData.Byarbejde;

namespace Administration.Option.Options.Logic
{
	public class SynchronizeFromCsv : AbstractDataOptionBase
	{
		public SynchronizeFromCsv(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeFromCsv = (DatabaseSynchronizeFromCsv)databaseOption;
		}

		private DatabaseSynchronizeFromCsv _databaseSynchronizeFromCsv;

		public static List<SynchronizeFromCsv> Find(MongoConnection connection)
		{
			List<DatabaseSynchronizeFromCsv> databaseSynchronizeFromCsvList = DatabaseOptionBase.ReadAllowed<DatabaseSynchronizeFromCsv>(connection);

			return databaseSynchronizeFromCsvList.Select(databaseSynchronizeFromCsv => new SynchronizeFromCsv(connection, databaseSynchronizeFromCsv)).ToList();
		}

		protected override bool ExecuteOption()
		{
			string fileName = _databaseSynchronizeFromCsv.fileName;
			string fileNameTmp = _databaseSynchronizeFromCsv.fileNameTmp;
			char delimeter = _databaseSynchronizeFromCsv.delimeter;
			string keyName = _databaseSynchronizeFromCsv.keyName;
			string dateName = _databaseSynchronizeFromCsv.dateName;
			string mappingField = _databaseSynchronizeFromCsv.mappingField;
			SystemInterface.Csv.ColumnDefinition[] fields = SystemInterface.Csv.ColumnDefinition.Read(_databaseSynchronizeFromCsv.fields);
			Guid changeProviderId = _databaseSynchronizeFromCsv.changeProviderId;
			DatabaseSynchronizeFromCsv.ImportTypeEnum importType = _databaseSynchronizeFromCsv.importType;

			if
			(
				fields.Any(definition => definition.Name == keyName) == false ||
				(
					dateName != null &&
					fields.Any(definition => definition.Name == dateName) == false
				)
			)
			{
				return false;
			}

			DateTime LatestModifiedDateTime;
			DateTime LatestModifiedDateTimeContact = ContactChange.GetLatestModifiedOn(SqlConnection, changeProviderId);
			DateTime LatestModifiedDateTimeAccount = AccountChange.GetLatestModifiedOn(SqlConnection, changeProviderId);
			if (LatestModifiedDateTimeAccount >= LatestModifiedDateTimeContact)
			{
				LatestModifiedDateTime = LatestModifiedDateTimeAccount;
			}
			else
			{
				LatestModifiedDateTime = LatestModifiedDateTimeContact;
			}

			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			List<Dictionary<string, object>> csvData;

			if (string.IsNullOrWhiteSpace(dateName))
			{
				csvData = csv.ReadAll();
			}
			else
			{
				csvData = csv.ReadLatest(keyName, dateName, LatestModifiedDateTime);
			}

			DataLayer.SqlData.ChangeProvider changeProvider = DataLayer.SqlData.ChangeProvider.Read(SqlConnection, changeProviderId);

			ProcessCsvData(changeProviderId, csvData, dateName, importType, keyName, mappingField);

			return true;
		}

		private void ProcessCsvData(Guid changeProviderId, List<Dictionary<string, object>> csvData, string dateName, DatabaseSynchronizeFromCsv.ImportTypeEnum importType, string keyName, string mappingField)
		{
			switch (importType)
			{
				case DatabaseSynchronizeFromCsv.ImportTypeEnum.Contact:
					ProcessCsvDataOnContact(changeProviderId, csvData, dateName, keyName);
					break;
				case DatabaseSynchronizeFromCsv.ImportTypeEnum.Account:
					ProcessCsvDataOnAccount(changeProviderId, csvData, dateName, keyName);
					break;
				case DatabaseSynchronizeFromCsv.ImportTypeEnum.AccountIndsamler:
					ProcessCsvDataOnAccountIndsamler(changeProviderId, csvData, dateName, keyName, mappingField);
					break;
				default:
					break;
			}
		}

		private void ProcessCsvDataOnContact(Guid changeProviderId, List<Dictionary<string, object>> csvData, string dateName, string keyName)
		{
			foreach (Dictionary<string, object> csvRow in csvData)
			{
				Guid externalContactId = GetIdFromRow(csvRow, keyName);

				Contact contact = ReadOrCreateContact(csvRow, externalContactId, changeProviderId, dateName);

				Guid contactId = contact.Id;

				DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName].ToString());

				bool ContactChangeExists = ContactChange.ContactChangeExists(SqlConnection, contactId, externalContactId, changeProviderId, collectedDate);

				if (ContactChangeExists == true)
				{
					continue;
				}

				CreateContactChange(changeProviderId, csvRow, externalContactId, contactId, collectedDate);
			}
		}

		private void ProcessCsvDataOnAccount(Guid changeProviderId, List<Dictionary<string, object>> csvData, string dateName, string keyName)
		{
			foreach (Dictionary<string, object> csvRow in csvData)
			{
				Guid externalAccountId = GetIdFromRow(csvRow, keyName);

				Account account = ReadOrCreateAccount(csvRow, externalAccountId, changeProviderId, dateName);

				Guid accountId = account.Id;

				DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName].ToString());

				bool AccountChangeExists = AccountChange.AccountChangeExists(SqlConnection, accountId, externalAccountId, changeProviderId, collectedDate);

				if (AccountChangeExists == true)
				{
					continue;
				}

				CreateAccountChange(changeProviderId, csvRow, externalAccountId, accountId, collectedDate);
			}
		}

		private void ProcessCsvDataOnAccountIndsamler(Guid changeProviderId, List<Dictionary<string, object>> csvData, string dateName, string keyName, string mappingField)
		{
			Dictionary<Guid, List<Guid>> externalContactsByExternalAccount = new Dictionary<Guid, List<Guid>>();

			DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(_databaseSynchronizeFromCsv.dateStringOverride);

			foreach (Dictionary<string, object> csvRow in csvData)
			{
				Guid externalAccountId = GetIdFromRow(csvRow, keyName);
				Guid externalContactId = GetIdFromRow(csvRow, mappingField);

				if (externalContactsByExternalAccount.ContainsKey(externalAccountId))
				{
					externalContactsByExternalAccount[externalAccountId].Add(externalContactId);
				}
				else
				{
					externalContactsByExternalAccount.Add(externalAccountId, new List<Guid>() { externalContactId });
				}
			}

			SetAccountIndsamler(externalContactsByExternalAccount, changeProviderId, dateName, collectedDate);
		}

		private Contact ReadOrCreateContact(Dictionary<string, object> csvRow, Guid externalContactId, Guid changeProviderId, string dateName)
		{
			bool externalContactExists = ExternalContact.Exists(SqlConnection, externalContactId, changeProviderId);

			ExternalContact externalContact = null;
			Contact contact = null;

			if (externalContactExists)
			{
				externalContact = ExternalContact.Read(SqlConnection, externalContactId, changeProviderId);
				contact = Contact.Read(SqlConnection, externalContact.ContactId);
			}
			else
			{
				contact = CreateContact(SqlConnection, csvRow, dateName);
				externalContact = new ExternalContact(SqlConnection, externalContactId, changeProviderId, contact.Id);
				externalContact.Insert();
			}

			return contact;
		}

		private Account ReadOrCreateAccount(Dictionary<string, object> csvRow, Guid externalAccountId, Guid changeProviderId, string dateName)
		{
			bool externalAccountExists = ExternalAccount.Exists(SqlConnection, externalAccountId, changeProviderId);

			ExternalAccount externalAccount = null;
			Account account = null;

			if (externalAccountExists)
			{
				externalAccount = ExternalAccount.Read(SqlConnection, externalAccountId, changeProviderId);
				account = Account.Read(SqlConnection, externalAccount.AccountId);
			}
			else
			{
				account = CreateAccount(SqlConnection, csvRow, dateName);
				externalAccount = new ExternalAccount(SqlConnection, externalAccountId, changeProviderId, account.Id);
				externalAccount.Insert();
			}

			return account;
		}

		private void SetAccountIndsamler(Dictionary<Guid, List<Guid>> externalContactsByExternalAccount, Guid changeProviderId, string dateName, DateTime collectedDate)
		{
			foreach (Guid externalAccountId in externalContactsByExternalAccount.Keys)
			{
				ExternalAccount externalAccount = ExternalAccount.Read(SqlConnection, externalAccountId, changeProviderId);

				Guid accountId = externalAccount.AccountId;

				List<Guid> externalContactsRequired = externalContactsByExternalAccount[externalAccountId];
				List<Guid> contactsRequiredGuid = externalContactsRequired.Select(externalContactId => ExternalContact.Read(SqlConnection, externalContactId, changeProviderId).ContactId).ToList();
				List<AccountIndsamler> accountIndsamlerRequired = contactsRequiredGuid.Select(contactId => new AccountIndsamler(accountId, contactId)).ToList();
				List<AccountIndsamler> accountIndsamlerExisting = AccountIndsamler.ReadFromAccountId(SqlConnection, accountId).ToList();
				List<AccountIndsamler> accountIndsamlerToRemove = accountIndsamlerExisting.Except(accountIndsamlerRequired).ToList();
				List<AccountIndsamler> accountIndsamlerToAdd = accountIndsamlerRequired.Except(accountIndsamlerExisting).ToList();

				accountIndsamlerToRemove.ForEach(accountIndsamler => accountIndsamler.Delete(SqlConnection));
				accountIndsamlerToAdd.ForEach(accountIndsamler => accountIndsamler.Insert(SqlConnection));

				SetAccountChangeIndsamler(changeProviderId, collectedDate, externalAccountId, accountId, contactsRequiredGuid);
			}
		}

		private void SetAccountChangeIndsamler(Guid changeProviderId, DateTime collectedDate, Guid externalAccountId, Guid accountId, List<Guid> contactsRequiredGuid)
		{
			bool AccountChangeExists = AccountChange.AccountChangeExists(SqlConnection, accountId, externalAccountId, changeProviderId, collectedDate);

			AccountChange accountChange;
			if (AccountChangeExists == false)
			{
				List<AccountChange> accountChanges = AccountChange.Read(SqlConnection, accountId, externalAccountId, changeProviderId, collectedDate);
				accountChanges = accountChanges.OrderBy(LAccountChange => (LAccountChange.modifiedon - collectedDate).Duration()).ToList();
				accountChange = accountChanges.FirstOrDefault();
			}
			else
			{
				accountChange = new AccountChange(SqlConnection, accountId, externalAccountId, changeProviderId);

				accountChange.createdon = collectedDate;
				accountChange.modifiedon = collectedDate;
				accountChange.Insert();
			}

			Guid accountChangeId = accountChange.Id;

			List<AccountChangeIndsamler> accountChangeIndsamlerRequired = contactsRequiredGuid.Select(contactId => new AccountChangeIndsamler(accountChangeId, contactId)).ToList();
			List<AccountChangeIndsamler> accountChangeIndsamlerExisting = AccountChangeIndsamler.ReadFromAccountChangeId(SqlConnection, accountChangeId).ToList();
			List<AccountChangeIndsamler> accountChangeIndsamlerToRemove = accountChangeIndsamlerExisting.Except(accountChangeIndsamlerRequired).ToList();
			List<AccountChangeIndsamler> accountChangeIndsamlerToAdd = accountChangeIndsamlerRequired.Except(accountChangeIndsamlerExisting).ToList();

			accountChangeIndsamlerToRemove.ForEach(accountChangeIndsamler => accountChangeIndsamler.Delete(SqlConnection));
			accountChangeIndsamlerToAdd.ForEach(accountChangeIndsamler => accountChangeIndsamler.Insert(SqlConnection));
		}

		private void CreateContactChange(Guid changeProviderId, Dictionary<string, object> csvRow, Guid externalContactId, Guid contactId, DateTime collectedDate)
		{
			ContactChange contactChange = new ContactChange(SqlConnection, contactId, externalContactId, changeProviderId);

			contactChange.createdon = collectedDate;
			contactChange.modifiedon = collectedDate;

			foreach (string key in csvRow.Keys)
			{
				Utilities.ReflectionHelper.SetValue(contactChange, key, csvRow[key]);
			}

			contactChange.Insert();

			if (csvRow.Keys.Contains("group") && csvRow["group"] != null && string.IsNullOrWhiteSpace(csvRow["group"].ToString()) == false)
			{
				Group group = Group.ReadByNameOrCreate(SqlConnection, csvRow["group"].ToString());
				ContactChangeGroup contactChangeGroup = new ContactChangeGroup(contactChange.Id, group.Id);
				contactChangeGroup.Insert(SqlConnection);
			}
		}

		private void CreateAccountChange(Guid changeProviderId, Dictionary<string, object> csvRow, Guid externalAccountId, Guid accountId, DateTime collectedDate)
		{
			AccountChange accountChange = new AccountChange(SqlConnection, accountId, externalAccountId, changeProviderId);

			accountChange.createdon = collectedDate;
			accountChange.modifiedon = collectedDate;

			IEnumerable<string> keys = csvRow.Keys.Except(new string[] { "group", "byarbejde", "kredsellerby", "region", "stedtype", "bykoordinatoremail", "omraadekoordinatoremail", "bykoordinatorkkadminmedlemsnr", "omraadekoordinatorkkadminmedlemsnr", "primarycontactkkadminmedlemsnr", "erindsamlingssted", "korshaerslederkkadminmedlemsnr", "genbrugskonsulentkkadminmedlemsnr", "indsamlingskoordinatorkkadminmedlemsnr" });

			foreach (string key in keys)
			{
				Utilities.ReflectionHelper.SetValue(accountChange, key, csvRow[key]);
			}

			if (csvRow.ContainsKey("bykoordinatoremail") && csvRow["bykoordinatoremail"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "emailaddress1", csvRow["bykoordinatoremail"].ToString());
				accountChange.bykoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("omraadekoordinatoremail") && csvRow["omraadekoordinatoremail"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "emailaddress1", csvRow["omraadekoordinatoremail"].ToString());
				accountChange.omraadekoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("bykoordinatorkkadminmedlemsnr") && csvRow["bykoordinatorkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["bykoordinatorkkadminmedlemsnr"].ToString());
				accountChange.bykoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("omraadekoordinatorkkadminmedlemsnr") && csvRow["omraadekoordinatorkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["omraadekoordinatorkkadminmedlemsnr"].ToString());
				accountChange.omraadekoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("primarycontactkkadminmedlemsnr") && csvRow["primarycontactkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["primarycontactkkadminmedlemsnr"].ToString());
				accountChange.primarycontact = contactId;
			}

			if (csvRow.ContainsKey("korshaerslederkkadminmedlemsnr") && csvRow["korshaerslederkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["korshaerslederkkadminmedlemsnr"].ToString());
				accountChange.korshaerslederid = contactId;
			}

			if (csvRow.ContainsKey("genbrugskonsulentkkadminmedlemsnr") && csvRow["genbrugskonsulentkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["genbrugskonsulentkkadminmedlemsnr"].ToString());
				accountChange.genbrugskonsulentid = contactId;
			}

			if (csvRow.ContainsKey("indsamlingskoordinatorkkadminmedlemsnr") && csvRow["indsamlingskoordinatorkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["indsamlingskoordinatorkkadminmedlemsnr"].ToString());
				accountChange.indsamlingskoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("kredsellerby") && csvRow["kredsellerby"] != null && string.IsNullOrWhiteSpace(csvRow["kredsellerby"].ToString()) == false)
			{
				object csvObject = csvRow["kredsellerby"];
				if (csvObject.GetType() == typeof(int))
				{
					accountChange.kredsellerby = (int)csvObject;
				}
				else
				{
					accountChange.kredsellerby = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.kredsellerbyEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.ContainsKey("region") && csvRow["region"] != null && string.IsNullOrWhiteSpace(csvRow["region"].ToString()) == false)
			{
				object csvObject = csvRow["region"];
				if (csvObject.GetType() == typeof(int))
				{
					accountChange.region = (int)csvObject;
				}
				else
				{
					accountChange.region = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.regionEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.ContainsKey("erindsamlingssted") && csvRow["erindsamlingssted"] != null && string.IsNullOrWhiteSpace(csvRow["erindsamlingssted"].ToString()) == false)
			{
				object csvObject = csvRow["erindsamlingssted"];
				if (csvObject.GetType() == typeof(int))
				{
					accountChange.erindsamlingssted = (int)csvObject;
				}
				else
				{
					accountChange.erindsamlingssted = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.erindsamlingsstedEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.ContainsKey("stedtype") && csvRow["stedtype"] != null && string.IsNullOrWhiteSpace(csvRow["stedtype"].ToString()) == false)
			{
				object csvObject = csvRow["stedtype"];
				if (csvObject.GetType() == typeof(int))
				{
					accountChange.stedtype = (int)csvObject;
				}
				else
				{
					accountChange.stedtype = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.stedtypeEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.Keys.Contains("byarbejde") && csvRow["byarbejde"] != null)
			{
				Byarbejde byarbejde = Byarbejde.ReadByNameOrCreate(SqlConnection, csvRow["byarbejde"].ToString());
				accountChange.byarbejdeid = byarbejde.Id;
			}

			accountChange.Insert();

			if (csvRow.Keys.Contains("group") && string.IsNullOrWhiteSpace(csvRow["group"].ToString()) == false)
			{
				Group group = Group.ReadByNameOrCreate(SqlConnection, csvRow["group"].ToString());
				AccountChangeGroup accountChangeGroup = new AccountChangeGroup(accountChange.Id, group.Id);
				accountChangeGroup.Insert(SqlConnection);
			}
		}

		private Contact CreateContact(SqlConnection sqlConnection, Dictionary<string, object> csvRow, string dateName)
		{
			DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName].ToString());

			Contact contact = new Contact()
			{
				createdon = DateTime.Now,
				modifiedon = collectedDate,
			};

			foreach (string key in csvRow.Keys)
			{
				Utilities.ReflectionHelper.SetValue(contact, key, csvRow[key]);
			}

			contact.Insert(SqlConnection);

			if (csvRow.Keys.Contains("group") && csvRow["group"] != null && string.IsNullOrWhiteSpace(csvRow["group"].ToString()) == false)
			{
				Group group = Group.ReadByNameOrCreate(SqlConnection, csvRow["group"].ToString());
				ContactGroup contactGroup = new ContactGroup(contact.Id, group.Id);
				contactGroup.Insert(SqlConnection);
			}

			return contact;
		}

		private Account CreateAccount(SqlConnection sqlConnection, Dictionary<string, object> csvRow, string dateName)
		{
			DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName].ToString());

			Account account = new Account()
			{
				createdon = DateTime.Now,
				modifiedon = collectedDate,
			};

			IEnumerable<string> keys = csvRow.Keys.Except(new string[] { "group", "byarbejde", "kredsellerby", "region", "stedtype", "bykoordinatoremail", "omraadekoordinatoremail", "bykoordinatorkkadminmedlemsnr", "omraadekoordinatorkkadminmedlemsnr", "primarycontactkkadminmedlemsnr", "erindsamlingssted", "korshaerslederkkadminmedlemsnr", "genbrugskonsulentkkadminmedlemsnr", "indsamlingskoordinatorkkadminmedlemsnr" });

			foreach (string key in keys)
			{
				Utilities.ReflectionHelper.SetValue(account, key, csvRow[key]);
			}

			if (csvRow.ContainsKey("bykoordinatoremail") && csvRow["bykoordinatoremail"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "emailaddress1", csvRow["bykoordinatoremail"].ToString());
				account.bykoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("omraadekoordinatoremail") && csvRow["omraadekoordinatoremail"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "emailaddress1", csvRow["omraadekoordinatoremail"].ToString());
				account.omraadekoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("bykoordinatorkkadminmedlemsnr") && csvRow["bykoordinatorkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["bykoordinatorkkadminmedlemsnr"].ToString());
				account.bykoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("omraadekoordinatorkkadminmedlemsnr") && csvRow["omraadekoordinatorkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["omraadekoordinatorkkadminmedlemsnr"].ToString());
				account.omraadekoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("primarycontactkkadminmedlemsnr") && csvRow["primarycontactkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["primarycontactkkadminmedlemsnr"].ToString());
				account.primarycontact = contactId;
			}

			if (csvRow.ContainsKey("korshaerslederkkadminmedlemsnr") && csvRow["korshaerslederkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["korshaerslederkkadminmedlemsnr"].ToString());
				account.korshaerslederid = contactId;
			}

			if (csvRow.ContainsKey("genbrugskonsulentkkadminmedlemsnr") && csvRow["genbrugskonsulentkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["genbrugskonsulentkkadminmedlemsnr"].ToString());
				account.genbrugskonsulentid = contactId;
			}

			if (csvRow.ContainsKey("indsamlingskoordinatorkkadminmedlemsnr") && csvRow["indsamlingskoordinatorkkadminmedlemsnr"] != null)
			{
				Guid? contactId = Contact.ReadIdFromField(SqlConnection, "new_kkadminmedlemsnr", csvRow["indsamlingskoordinatorkkadminmedlemsnr"].ToString());
				account.indsamlingskoordinatorid = contactId;
			}

			if (csvRow.ContainsKey("kredsellerby") && csvRow["kredsellerby"] != null && string.IsNullOrWhiteSpace(csvRow["kredsellerby"].ToString()) == false)
			{
				object csvObject = csvRow["kredsellerby"];
				if (csvObject.GetType() == typeof(int))
				{
					account.kredsellerby = (int)csvObject;
				}
				else
				{
					account.kredsellerby = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.kredsellerbyEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.ContainsKey("region") && csvRow["region"] != null && string.IsNullOrWhiteSpace(csvRow["region"].ToString()) == false)
			{
				object csvObject = csvRow["region"];
				if (csvObject.GetType() == typeof(int))
				{
					account.region = (int)csvObject;
				}
				else
				{
					account.region = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.regionEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.ContainsKey("erindsamlingssted") && csvRow["erindsamlingssted"] != null && string.IsNullOrWhiteSpace(csvRow["erindsamlingssted"].ToString()) == false)
			{
				object csvObject = csvRow["erindsamlingssted"];
				if (csvObject.GetType() == typeof(int))
				{
					account.erindsamlingssted = (int)csvObject;
				}
				else
				{
					account.erindsamlingssted = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.erindsamlingsstedEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.ContainsKey("stedtype") && csvRow["stedtype"] != null && string.IsNullOrWhiteSpace(csvRow["stedtype"].ToString()) == false)
			{
				object csvObject = csvRow["stedtype"];
				if (csvObject.GetType() == typeof(int))
				{
					account.stedtype = (int)csvObject;
				}
				else
				{
					account.stedtype = (int)Enum.Parse(typeof(SystemInterface.Dynamics.Crm.Account.stedtypeEnum), csvObject.ToString(), true);
				}
			}

			if (csvRow.Keys.Contains("byarbejde") && csvRow["byarbejde"] != null)
			{
				Byarbejde byarbejde = Byarbejde.ReadByNameOrCreate(SqlConnection, csvRow["byarbejde"].ToString());
				account.byarbejdeid = byarbejde.Id;
			}

			account.Insert(SqlConnection);

			if (csvRow.Keys.Contains("group") && string.IsNullOrWhiteSpace(csvRow["group"].ToString()) == false)
			{
				Group group = Group.ReadByNameOrCreate(SqlConnection, csvRow["group"].ToString());
				AccountGroup accountGroup = new AccountGroup(account.Id, group.Id);
				accountGroup.Insert(SqlConnection);
			}

			return account;
		}

		private Guid GetIdFromRow(Dictionary<string, object> csvRow, string keyName)
		{
			int idInt;
			int.TryParse(csvRow[keyName].ToString(), out idInt);
			Guid id = Utilities.Converter.GuidConverter.Convert(0, 0, 0, idInt);

			return id;
		}
	}
}
