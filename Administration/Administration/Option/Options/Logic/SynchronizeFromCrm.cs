using Administration.Option.Options.Data;
using System;
using DatabaseSynchronizeFromCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DataLayer;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using Administration.Mapping.Contact;
using System.Data.SqlClient;
using Administration.Mapping.Account;

namespace Administration.Option.Options.Logic
{
	public class SynchronizeFromCrm : AbstractDataOptionBase
	{
		private DatabaseSynchronizeFromCrm _databaseSynchronizeFromCrm;

		public SynchronizeFromCrm(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeFromCrm = (DatabaseSynchronizeFromCrm)databaseOption;
		}

		public static List<SynchronizeFromCrm> Find(MongoConnection connection)
		{
			List<DatabaseSynchronizeFromCrm> databaseSynchronizeFromCrmList = DatabaseOptionBase.ReadAllowed<DatabaseSynchronizeFromCrm>(connection);

			return databaseSynchronizeFromCrmList.Select(databaseSynchronizeFromCrm => new SynchronizeFromCrm(connection, databaseSynchronizeFromCrm)).ToList();
		}

		protected override bool ExecuteOption()
		{
			Guid changeProviderId = _databaseSynchronizeFromCrm.changeProviderId;
			string urlLoginName = _databaseSynchronizeFromCrm.urlLoginName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection connection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			SynchronizeContacts(changeProviderId, connection);

			SynchronizeAccounts(changeProviderId, connection);

			return true;
		}

		private void SynchronizeContacts(Guid changeProviderId, DynamicsCrmConnection connection)
		{
			DataLayer.MongoData.Progress progress;
			DateTime searchDate = GetSearchDateContact(out progress);

			List<Contact> contacts = Contact.ReadLatest(connection, searchDate);

			contacts.ForEach(contact => StoreInContactChangesIfNeeded(contact, changeProviderId));

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private void SynchronizeAccounts(Guid changeProviderId, DynamicsCrmConnection connection)
		{
			DataLayer.MongoData.Progress progress;
			DateTime searchDate = GetSearchDateAccount(out progress);

			List<Account> accounts = Account.ReadLatest(connection, searchDate);

			accounts.ForEach(account => StoreInAccountChangesIfNeeded(account, changeProviderId));

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		internal void StoreInContactChangesIfNeeded(Contact crmContact, Guid changeProviderId)
		{
			Guid externalContactId = crmContact.Id;
			DatabaseContact contact = ReadOrCreateContact(crmContact, externalContactId);

			DataLayer.SqlData.Contact.ExternalContact externalContact = DataLayer.SqlData.Contact.ExternalContact.ReadOrCreate(SqlConnection, externalContactId, changeProviderId, contact.Id);

			StoreInContactChangesIfNeeded(crmContact, changeProviderId, externalContactId, contact);
		}

		internal void StoreInAccountChangesIfNeeded(Account crmAccount, Guid changeProviderId)
		{
			Guid externalAccountId = crmAccount.Id;
			DatabaseAccount account = ReadOrCreateAccount(crmAccount, externalAccountId);

			DataLayer.SqlData.Account.ExternalAccount externalAccount = DataLayer.SqlData.Account.ExternalAccount.ReadOrCreate(SqlConnection, externalAccountId, changeProviderId, account.Id);

			StoreInAccountChangesIfNeeded(crmAccount, changeProviderId, externalAccountId, account);
		}

		internal void StoreInContactChangesIfNeeded(Contact crmContact, Guid changeProviderId, Guid externalContactId, DatabaseContact contact)
		{
			Guid contactId = contact.Id;
			DateTime modifiedOn = crmContact.modifiedon;

			bool ContactChangeExists = DatabaseContactChange.ContactChangeExists(SqlConnection, contactId, externalContactId, changeProviderId, modifiedOn);

			if (ContactChangeExists == true)
			{
				return;
			}

			CreateContactChange(changeProviderId, crmContact, externalContactId, contactId, modifiedOn);
		}

		internal void StoreInAccountChangesIfNeeded(Account crmAccount, Guid changeProviderId, Guid externalAccountId, DatabaseAccount account)
		{
			Guid accountId = account.Id;
			DateTime modifiedOn = crmAccount.modifiedon;

			bool AccountChangeExists = DatabaseAccountChange.AccountChangeExists(SqlConnection, accountId, externalAccountId, changeProviderId, modifiedOn);

			if (AccountChangeExists == true)
			{
				return;
			}

			CreateAccountChange(changeProviderId, crmAccount, externalAccountId, accountId, modifiedOn);
		}

		private DatabaseContact ReadOrCreateContact(Contact crmContact, Guid externalContactId)
		{
			DatabaseContact contact = ContactCrmMapping.FindContact(Connection, SqlConnection, externalContactId, crmContact);

			if (contact == null)
			{
				contact = CreateContact(SqlConnection, crmContact);
			}

			return contact;
		}

		private DatabaseAccount ReadOrCreateAccount(Account crmAccount, Guid externalAccountId)
		{
			DatabaseAccount account = AccountCrmMapping.FindAccount(Connection, SqlConnection, externalAccountId, crmAccount);

			if (account == null)
			{
				account = CreateAccount(SqlConnection, crmAccount);
			}

			return account;
		}

		private DatabaseContact CreateContact(SqlConnection sqlConnection, Contact crmContact)
		{
			DatabaseContact contact = new DatabaseContact()
			{
				CreatedOn = DateTime.Now,
				ModifiedOn = crmContact.modifiedon,
				Firstname = crmContact.firstname,
				Lastname = crmContact.lastname,
			};

			contact.Insert(sqlConnection);

			return contact;
		}

		private DatabaseAccount CreateAccount(SqlConnection sqlConnection, Account crmAccount)
		{
			DatabaseAccount account = new DatabaseAccount()
			{
				CreatedOn = DateTime.Now,
				ModifiedOn = crmAccount.modifiedon,
				name = crmAccount.name,
			};

			account.Insert(sqlConnection);

			return account;
		}

		private void CreateContactChange(Guid changeProviderId, Contact crmContact, Guid externalContactId, Guid contactId, DateTime modifiedOn)
		{
			DatabaseContactChange contactChange = new DatabaseContactChange(SqlConnection, contactId, externalContactId, changeProviderId);

			contactChange.CreatedOn = crmContact.createdon;
			contactChange.ModifiedOn = crmContact.modifiedon;
			contactChange.Firstname = crmContact.firstname;
			contactChange.Lastname = crmContact.lastname;

			contactChange.Insert();
		}

		private void CreateAccountChange(Guid changeProviderId, Account crmAccount, Guid externalAccountId, Guid accountId, DateTime modifiedOn)
		{
			DatabaseAccountChange accountChange = new DatabaseAccountChange(SqlConnection, accountId, externalAccountId, changeProviderId);

			accountChange.CreatedOn = crmAccount.createdon;
			accountChange.ModifiedOn = crmAccount.modifiedon;
			accountChange.name = crmAccount.name;

			accountChange.Insert();
		}

		private DateTime GetSearchDateContact(out DataLayer.MongoData.Progress progress)
		{
			string progressName = "DynamicsCrmContactFrom";

			progress = DataLayer.MongoData.Progress.ReadNext(Connection, progressName);

			if (progress == null)
			{
				progress = new DataLayer.MongoData.Progress()
				{
					LastProgressDate = DateTime.MinValue,
					TargetId = Guid.Empty,
					TargetName = progressName,
				};

				progress.Insert(Connection);
			}

			DateTime searchDate = progress.LastProgressDate;

			if (searchDate <= DateTime.MinValue.AddHours(1))
			{
				return DateTime.MinValue;
			}

			return progress.LastProgressDate.AddHours(-1);
		}

		private DateTime GetSearchDateAccount(out DataLayer.MongoData.Progress progress)
		{
			string progressName = "DynamicsCrmAccountFrom";

			progress = DataLayer.MongoData.Progress.ReadNext(Connection, progressName);

			if (progress == null)
			{
				progress = new DataLayer.MongoData.Progress()
				{
					LastProgressDate = DateTime.MinValue,
					TargetId = Guid.Empty,
					TargetName = progressName,
				};

				progress.Insert(Connection);
			}

			DateTime searchDate = progress.LastProgressDate;

			if (searchDate <= DateTime.MinValue.AddHours(1))
			{
				return DateTime.MinValue;
			}

			return progress.LastProgressDate.AddHours(-1);
		}
	}
}
