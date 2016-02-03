using Administration.Option.Options.Data;
using System;
using System.Collections.Generic;
using DatabaseSynchronizeToCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseAccountContact = DataLayer.SqlData.Account.AccountContact;
using DatabaseAccountGroup = DataLayer.SqlData.Group.AccountGroup;
using DatabaseContactGroup = DataLayer.SqlData.Group.ContactGroup;
using DatabaseGroup = DataLayer.SqlData.Group.Group;
using DatabaseAccountIndsamler = DataLayer.SqlData.Account.AccountIndsamler;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using SystemInterfaceGroup = SystemInterface.Dynamics.Crm.Group;
using DataLayer;
using SystemInterface.Dynamics.Crm;
using Administration.Mapping.Contact;
using System.Linq;
using Administration.Mapping.Account;
using Utilities.Comparer;
using DataLayer.SqlData.Account;

namespace Administration.Option.Options.Logic
{
	public class SynchronizeToCrm : AbstractDataOptionBase
	{
		private DatabaseSynchronizeToCrm _databaseSynchronizeToCrm;
		private DynamicsCrmConnection _dynamicsCrmConnection;
		private SynchronizeFromCrm _synchronizeFromCrm;
		private Squash _squash;

		public SynchronizeToCrm(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeToCrm = (DatabaseSynchronizeToCrm)databaseOption;
			string urlLoginName = _databaseSynchronizeToCrm.urlLoginName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			_synchronizeFromCrm = new SynchronizeFromCrm(connection, null);
			_squash = new Squash(connection, null);
		}

		public static List<SynchronizeToCrm> Find(MongoConnection connection)
		{
			List<DatabaseSynchronizeToCrm> options = DatabaseOptionBase.ReadAllowed<DatabaseSynchronizeToCrm>(connection);

			return options.Select(option => new SynchronizeToCrm(connection, option)).ToList();
		}

		protected override bool ExecuteOption()
		{
			Guid changeProviderId = _databaseSynchronizeToCrm.changeProviderId;

			SynchronizeContacts(changeProviderId);

			SynchronizeAccounts(changeProviderId);

			return true;
		}

		private void SynchronizeContacts(Guid changeProviderId)
		{
			DataLayer.MongoData.Progress progress;
			DatabaseContact databaseContact = GetContactToSynchronize(out progress);

			if (databaseContact == null)
			{
				return;
			}

			List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(SqlConnection, changeProviderId, databaseContact.Id);

			if (externalContacts.Count == 0)
			{
				InsertContactAndCreateExternalContact(changeProviderId, databaseContact);
			}
			else
			{
				externalContacts.ForEach(contact => UpdateExternalContactIfNeeded(changeProviderId, contact, databaseContact));
			}

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private void SynchronizeAccounts(Guid changeProviderId)
		{
			DataLayer.MongoData.Progress progress;
			DatabaseAccount databaseAccount = GetAccountToSynchronize(out progress);

			if (databaseAccount == null)
			{
				return;
			}

			List<DatabaseExternalAccount> externalAccounts = DatabaseExternalAccount.ReadFromChangeProviderAndAccount(SqlConnection, changeProviderId, databaseAccount.Id);

			if (externalAccounts.Count == 0)
			{
				InsertAccountAndCreateExternalAccount(changeProviderId, databaseAccount);
			}
			else
			{
				externalAccounts.ForEach(account => UpdateExternalAccountIfNeeded(changeProviderId, account, databaseAccount));
			}

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private void InsertContactAndCreateExternalContact(Guid changeProviderId, DatabaseContact databaseContact)
		{
			SystemInterfaceContact systemInterfaceContact = Conversion.Contact.Convert(_dynamicsCrmConnection, databaseContact);
			systemInterfaceContact.Insert();

			DatabaseExternalContact externalContact = new DatabaseExternalContact(SqlConnection, systemInterfaceContact.Id, changeProviderId, databaseContact.Id);
			externalContact.Insert();
		}

		private void InsertAccountAndCreateExternalAccount(Guid changeProviderId, DatabaseAccount databaseAccount)
		{
			SystemInterfaceAccount systemInterfaceAccount = Conversion.Account.Convert(_dynamicsCrmConnection, databaseAccount);
			systemInterfaceAccount.Insert();

			DatabaseExternalAccount externalAccount = new DatabaseExternalAccount(SqlConnection, systemInterfaceAccount.Id, changeProviderId, databaseAccount.Id);
			externalAccount.Insert();
		}

		private void UpdateExternalContactIfNeeded(Guid changeProviderId, DatabaseExternalContact databaseExternalContact, DatabaseContact databaseContact)
		{
			SystemInterfaceContact systemInterfaceContactInCrm = SystemInterfaceContact.Read(_dynamicsCrmConnection, databaseExternalContact.ExternalContactId);

			SystemInterfaceContact systemInterfaceContact = Conversion.Contact.Convert(_dynamicsCrmConnection, databaseContact);

			if (systemInterfaceContactInCrm.Equals(systemInterfaceContact))
			{
				return;
			}

			systemInterfaceContactInCrm.SetActive(false);

			systemInterfaceContactInCrm = SystemInterfaceContact.Read(_dynamicsCrmConnection, databaseExternalContact.ExternalContactId);

			_synchronizeFromCrm.StoreInContactChangesIfNeeded(systemInterfaceContactInCrm, changeProviderId, databaseExternalContact.ExternalContactId, databaseContact);

			databaseContact = DatabaseContact.Read(SqlConnection, databaseContact.Id);

			_squash.SquashContact(databaseContact);

			Conversion.Contact.Convert(_dynamicsCrmConnection, databaseContact, systemInterfaceContactInCrm);

			systemInterfaceContactInCrm.Update();

			systemInterfaceContactInCrm.SetActive(true);
		}

		private void UpdateExternalAccountIfNeeded(Guid changeProviderId, DatabaseExternalAccount databaseExternalAccount, DatabaseAccount databaseAccount)
		{
			SystemInterfaceAccount systemInterfaceAccountInCrm = SystemInterfaceAccount.Read(_dynamicsCrmConnection, databaseExternalAccount.ExternalAccountId);

			SystemInterfaceAccount systemInterfaceAccount = Conversion.Account.Convert(_dynamicsCrmConnection, databaseAccount);

			if (systemInterfaceAccountInCrm.Equals(systemInterfaceAccount))
			{
				return;
			}

			systemInterfaceAccountInCrm.SetActive(false);

			systemInterfaceAccountInCrm = SystemInterfaceAccount.Read(_dynamicsCrmConnection, databaseExternalAccount.ExternalAccountId);

			_synchronizeFromCrm.StoreInAccountChangesIfNeeded(systemInterfaceAccountInCrm, changeProviderId, databaseExternalAccount.ExternalAccountId, databaseAccount);

			databaseAccount = DatabaseAccount.Read(SqlConnection, databaseAccount.Id);

			_squash.SquashAccount(databaseAccount);

			Conversion.Account.Convert(_dynamicsCrmConnection, databaseAccount, systemInterfaceAccountInCrm);

			systemInterfaceAccountInCrm.Update();

			systemInterfaceAccountInCrm.SetActive(true);
		}

		private DatabaseContact GetContactToSynchronize(out DataLayer.MongoData.Progress progress)
		{
			progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressContactToCrm);

			if (progress == null)
			{
				return null;
			}

			Guid contactId = progress.TargetId;

			DatabaseContact contact = DatabaseContact.Read(SqlConnection, contactId);

			return contact;
		}

		private DatabaseAccount GetAccountToSynchronize(out DataLayer.MongoData.Progress progress)
		{
			progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressAccountToCrm);

			if (progress == null)
			{
				return null;
			}

			Guid accountId = progress.TargetId;

			DatabaseAccount account = DatabaseAccount.Read(SqlConnection, accountId);

			return account;
		}

		private bool InsertAccountContact(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId)
		{
			List<DatabaseAccountContact> accountContacts = DatabaseAccountContact.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseAccount> databaseAccounts = accountContacts.Select(accountContact => DatabaseAccount.Read(SqlConnection, accountContact.AccountId)).ToList();

			List<Guid> accountIds = GetExternalAccountIdsFromDatabaseAccounts(changeProviderId, databaseAccounts);

			systemInterfaceContact.SynchronizeAccounts(accountIds);

			return true;
		}

		private void InsertContactGroup(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact)
		{
			List<DatabaseContactGroup> contactGroups = DatabaseContactGroup.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseGroup> databaseGroups = contactGroups.Select(contactGroup => DatabaseGroup.Read(SqlConnection, contactGroup.GroupId)).ToList();

			List<string> groupNames = databaseGroups.Select(group => group.Name).ToList();
			systemInterfaceContact.SynchronizeGroups(groupNames);
		}

		private void InsertAccountIndsamler(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId)
		{
			List<DatabaseAccountIndsamler> accountIndsamlere = DatabaseAccountIndsamler.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseAccount> databaseAccounts = accountIndsamlere.Select(accountContact => DatabaseAccount.Read(SqlConnection, accountContact.AccountId)).ToList();

			List<Guid> accountIds = GetExternalAccountIdsFromDatabaseAccounts(changeProviderId, databaseAccounts);

			systemInterfaceContact.SynchronizeIndsamlere(accountIds);
		}

		private List<Guid> GetExternalAccountIdsFromDatabaseAccounts(Guid changeProviderId, List<DatabaseAccount> databaseAccounts)
		{
			List<DatabaseExternalAccount> databaseExternalAccounts = databaseAccounts.SelectMany(databaseAccount => AccountCrmMapping.FindAccounts(Connection, SqlConnection, databaseAccount, changeProviderId)).ToList();

			List<Guid> accountIds = databaseExternalAccounts.Select(databaseExternalAccount => databaseExternalAccount.ExternalAccountId).ToList();
			return accountIds;
		}

		private void InsertAccountContact(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId)
		{
			List<DatabaseAccountContact> accountContacts = DatabaseAccountContact.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseContact> databaseContacts = accountContacts.Select(accountContact => DatabaseContact.Read(SqlConnection, accountContact.ContactId)).ToList();

			List<Guid> contactIds = GetExternalContactIdsFromDatabaseContacts(changeProviderId, databaseContacts);

			systemInterfaceAccount.SynchronizeContacts(contactIds);
		}

		private void InsertAccountIndsamler(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId)
		{
			List<DatabaseAccountIndsamler> accountIndsamlere = DatabaseAccountIndsamler.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseContact> databaseContacts = accountIndsamlere.Select(accountContact => DatabaseContact.Read(SqlConnection, accountContact.ContactId)).ToList();

			List<Guid> contactIds = GetExternalContactIdsFromDatabaseContacts(changeProviderId, databaseContacts);

			systemInterfaceAccount.SynchronizeIndsamlere(contactIds);
		}

		private void InsertAccountGroup(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount)
		{
			List<DatabaseAccountGroup> accountGroups = DatabaseAccountGroup.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseGroup> databaseGroups = accountGroups.Select(accountGroup => DatabaseGroup.Read(SqlConnection, accountGroup.GroupId)).ToList();

			List<SystemInterfaceGroup> systemInterfaceGroups = databaseGroups.Select(databaseGroup => SystemInterfaceGroup.ReadOrCreate(_dynamicsCrmConnection, databaseGroup.Name)).ToList();

			List<Guid> groupIds = systemInterfaceGroups.Select(group => group.GroupId).ToList();

			systemInterfaceAccount.SynchronizeGroups(groupIds);
		}

		private List<Guid> GetExternalContactIdsFromDatabaseContacts(Guid changeProviderId, List<DatabaseContact> databaseContacts)
		{
			List<DatabaseExternalContact> databaseExternalContacts = databaseContacts.SelectMany(databaseContact => ContactCrmMapping.FindContacts(Connection, SqlConnection, databaseContact, changeProviderId)).ToList();

			List<Guid> contactIds = databaseExternalContacts.Select(databaseExternalContact => databaseExternalContact.ExternalContactId).ToList();
			return contactIds;
		}
	}
}
