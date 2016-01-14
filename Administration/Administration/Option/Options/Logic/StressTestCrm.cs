using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using DatabaseStressTestCrm = DataLayer.MongoData.Option.Options.Logic.StressTestCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using SystemInterface.Dynamics.Crm;
using DataLayer.MongoData.Statistics.StringStatistics;
using System.Linq;
using Administration.Mapping.RandomData;

namespace Administration.Option.Options.Logic
{
	public class StressTestCrm : AbstractDataOptionBase
	{
		private DatabaseStressTestCrm _databaseStressTestCrm;
		private DynamicsCrmConnection _dynamicsCrmConnection;

		public StressTestCrm(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseStressTestCrm = (DatabaseStressTestCrm)DatabaseOption;

			string urlLoginName = _databaseStressTestCrm.urlLoginName;
			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
		}

		public SystemInterfaceContact CreateContact()
		{
			SystemInterfaceContact contact = new SystemInterfaceContact(_dynamicsCrmConnection);

			RandomContact.GetInstance().AnonymizeContact(contact);

			contact.Groups = contact.Groups.Select(group => Group.ReadOrCreate(_dynamicsCrmConnection, group.Name)).ToList();

			contact.Insert();

			return contact;
		}

		public void AddAccountRelatedContacts(SystemInterfaceAccount account, List<SystemInterfaceContact> contacts)
		{
			Random random = new Random();

			List<SystemInterfaceContact> indsamlere = contacts.Where(contact => RandomAccount.GetInstance().GetRandomBool(random, 25)).ToList();
			List<SystemInterfaceContact> related = contacts.Where(contact => RandomAccount.GetInstance().GetRandomBool(random, 25)).ToList();

			account.SynchronizeIndsamlere(indsamlere);
			account.SynchronizeContacts(related);
		}

		public SystemInterfaceAccount CreateAccount(List<SystemInterfaceContact> contacts)
		{
			SystemInterfaceAccount account = new SystemInterfaceAccount(_dynamicsCrmConnection);

			RandomAccount.GetInstance().AnonymizeAccount(account);

			account.Insert();

			List<Group> groups = RandomGroup.GetInstance().GetRandomGroups();
			groups = groups.Select(group => Group.ReadOrCreate(_dynamicsCrmConnection, group.Name)).ToList();
			account.SynchronizeGroups(groups);

			AddAccountRelatedContacts(account, contacts);

			return account;
		}

		public static List<StressTestCrm> Find(MongoConnection connection)
		{
			List<DatabaseStressTestCrm> options = DatabaseOptionBase.ReadAllowed<DatabaseStressTestCrm>(connection);

			return options.Select(option => new StressTestCrm(connection, option)).ToList();
		}

		protected override bool ExecuteOption()
		{
			int contactsToCreate = _databaseStressTestCrm.contactsToCreate;
			int accountsToCreate = _databaseStressTestCrm.accountsToCreate;
			DataLayer.MongoData.IntProgress intProgress = GetIntProgress();

			if (intProgress.progressValue == 0)
			{
				bool areContactsDeleted = false;
				while (areContactsDeleted == false)
				{
					areContactsDeleted = DeleteAllContacts(contactsToCreate);
				}

				bool areAccountsDeleted = false;
				while (areAccountsDeleted == false)
				{
					areAccountsDeleted = DeleteAllAccounts(accountsToCreate);
				}
			}

			DateTime BeforeInsert = DateTime.Now;

			List<SystemInterfaceContact> contactsCreated = new List<SystemInterfaceContact>();

			for (int contactsIndex = 0; contactsIndex < contactsToCreate; contactsIndex++)
			{
				SystemInterfaceContact contact = CreateContact();

				contactsCreated.Add(contact);
			}

			for (int accountIndex = 0; accountIndex < accountsToCreate; accountIndex++)
			{
				CreateAccount(contactsCreated);
			}

			DateTime AfterInsert = DateTime.Now;

			int totalMilliseconds = (int)((AfterInsert - BeforeInsert).TotalMilliseconds);
			int milliSecondsForEachContact = totalMilliseconds / (contactsToCreate + accountsToCreate);

			Log.Write(Connection, $"inserted {contactsToCreate} contacts and {accountsToCreate} accounts in {totalMilliseconds} milli seconds", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			int step = 1000;
			int min = (intProgress.progressValue / step) * step;
			int max = min + step;

			StringIntStatistics.Create(Connection, intProgress.TargetName, $"{min} - {max}", milliSecondsForEachContact);

			intProgress.progressValue += contactsToCreate;
			intProgress.Update(Connection);

			return true;
		}

		private DataLayer.MongoData.IntProgress GetIntProgress()
		{
			string targetName = "StressTestCrm";
			Guid targetId = Guid.Empty;

			DataLayer.MongoData.IntProgress intProgress;
			if (DataLayer.MongoData.IntProgress.Exists(Connection, targetName, targetId))
			{
				intProgress = DataLayer.MongoData.IntProgress.Read(Connection, targetName, targetId);
			}
			else
			{
				intProgress = new DataLayer.MongoData.IntProgress()
				{
					progressValue = 0,
					TargetId = targetId,
					TargetName = targetName,
				};

				intProgress.Insert(Connection);
			}

			return intProgress;
		}

		private bool DeleteAllContacts(int maxNumberOfContactsToDelete)
		{
			List<SystemInterfaceContact> contacts = SystemInterfaceContact.ReadLatest(_dynamicsCrmConnection, DateTime.MinValue, maxNumberOfContactsToDelete);

			Log.Write(Connection, $"starting to delete {contacts.Count} contacts", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			foreach (SystemInterfaceContact contact in contacts)
			{
				contact.Delete();
			}

			Log.Write(Connection, $"{contacts.Count} contacts deleted", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			if (maxNumberOfContactsToDelete == contacts.Count)
			{
				return false;
			}

			return true;
		}

		private bool DeleteAllAccounts(int maxNumberOfAccountsToDelete)
		{
			List<SystemInterfaceAccount> accounts = SystemInterfaceAccount.ReadLatest(_dynamicsCrmConnection, DateTime.MinValue, maxNumberOfAccountsToDelete);

			Log.Write(Connection, $"starting to delete {accounts.Count} accounts", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			foreach (SystemInterfaceAccount account in accounts)
			{
				account.Delete();
			}

			Log.Write(Connection, $"{accounts.Count} accounts deleted", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			if (maxNumberOfAccountsToDelete == accounts.Count)
			{
				return false;
			}

			return true;
		}
	}
}
