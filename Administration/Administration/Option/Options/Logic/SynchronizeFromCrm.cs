using Administration.Option.Options.Data;
using System;
using DatabaseSynchronizeFromCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseAccountIndsamler = DataLayer.SqlData.Account.AccountIndsamler;
using DatabaseContactChangeGroup = DataLayer.SqlData.Group.ContactChangeGroup;
using DatabaseAccountChangeGroup = DataLayer.SqlData.Group.AccountChangeGroup;
using DatabaseGroup = DataLayer.SqlData.Group.Group;
using DatabaseAccountChangeContact = DataLayer.SqlData.Account.AccountChangeContact;
using DatabaseAccountChangeIndsamler = DataLayer.SqlData.Account.AccountChangeIndsamler;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseByarbejde = DataLayer.SqlData.Byarbejde.Byarbejde;
using DatabaseExternalByarbejde = DataLayer.SqlData.Byarbejde.ExternalByarbejde;
using DataLayer;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using Administration.Mapping.Contact;
using System.Data.SqlClient;
using Administration.Mapping.Account;
using DataLayer.SqlData.Annotation;

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
			bool changeProviderExists = DatabaseChangeProvider.Exists(SqlConnection, changeProviderId);
			if (changeProviderExists == false)
			{
				throw new ArgumentException($"Could not find changeprovider with id {changeProviderId}");
			}

			string urlLoginName = _databaseSynchronizeFromCrm.urlLoginName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection connection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
			SystemUser systemUser = SystemUser.ReadByDomainname(connection, login.Username);

			SynchronizeContacts(changeProviderId, connection, systemUser);

			SynchronizeAccounts(changeProviderId, connection, systemUser);

			return true;
		}

		private void SynchronizeContacts(Guid changeProviderId, DynamicsCrmConnection connection, SystemUser systemUser)
		{
			DataLayer.MongoData.Progress progress;
			DateTime searchDate = GetSearchDateContact(out progress);

			List<Contact> contacts = Contact.ReadLatest(connection, searchDate, _databaseSynchronizeFromCrm.maxNumberOfContacts);

			if (_databaseSynchronizeFromCrm.ignoreChangesMadeBySystemUser)
			{
				contacts = contacts.Where(contact => contact.modifiedbyGuid == null || contact.modifiedbyGuid.Value != systemUser.Id).ToList();
			}

			contacts.ForEach(contact => StoreInContactChangesIfNeeded(contact, changeProviderId));

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private void SynchronizeAccounts(Guid changeProviderId, DynamicsCrmConnection connection, SystemUser systemUser)
		{
			DataLayer.MongoData.Progress progress;
			DateTime searchDate = GetSearchDateAccount(out progress);

			List<Account> accounts = Account.ReadLatest(connection, searchDate, _databaseSynchronizeFromCrm.maxNumberOfAccounts);

			accounts.ForEach(account => StoreByarbejdeIfNeeded(account, changeProviderId, connection));

			accounts.ForEach(account => StoreInAccountChangesIfNeeded(account, changeProviderId, connection));

			if (_databaseSynchronizeFromCrm.ignoreChangesMadeBySystemUser)
			{
				accounts = accounts.Where(account => account.modifiedbyGuid == null || account.modifiedbyGuid.Value != systemUser.Id).ToList();
			}

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		internal void StoreInContactChangesIfNeeded(Contact crmContact, Guid changeProviderId)
		{
			Guid externalContactId = crmContact.Id;

			bool externalContactExists = DatabaseExternalContact.Exists(SqlConnection, externalContactId, changeProviderId);

			DatabaseExternalContact externalContact = null;
			DatabaseContact contact = null;

			if (externalContactExists)
			{
				externalContact = DatabaseExternalContact.Read(SqlConnection, externalContactId, changeProviderId);
				contact = DatabaseContact.Read(SqlConnection, externalContact.ContactId);
			}
			else
			{
				contact = ReadOrCreateContact(crmContact);

				externalContact = new DatabaseExternalContact(SqlConnection, externalContactId, changeProviderId, contact.Id);
				externalContact.Insert();
			}

			StoreInContactChangesIfNeeded(crmContact, changeProviderId, externalContactId, contact);
		}

		internal void StoreInAccountChangesIfNeeded(Account crmAccount, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			Guid externalAccountId = crmAccount.Id;

			bool externalAccountExists = DatabaseExternalAccount.Exists(SqlConnection, externalAccountId, changeProviderId);

			DatabaseAccount account = null;
			DatabaseExternalAccount externalAccount = null;

			if (externalAccountExists)
			{
				externalAccount = DatabaseExternalAccount.Read(SqlConnection, externalAccountId, changeProviderId);
				account = DatabaseAccount.Read(SqlConnection, externalAccount.AccountId);
			}
			else
			{
				account = ReadOrCreateAccount(crmAccount, changeProviderId);

				externalAccount = new DatabaseExternalAccount(SqlConnection, externalAccountId, changeProviderId, account.Id);
				externalAccount.Insert();
			}

			StoreInAccountChangesIfNeeded(crmAccount, changeProviderId, externalAccountId, account, dynamicsCrmConnection);
		}

		private void StoreByarbejdeIfNeeded(Account crmAccount, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			Guid? externalByarbejdeId = crmAccount.byarbejdeid;

			if (externalByarbejdeId.HasValue == false)
			{
				return;
			}

			Byarbejde crmByarbejde = Byarbejde.Read(dynamicsCrmConnection, externalByarbejdeId.Value);

			if (crmByarbejde == null)
			{
				return;
			}

			List<DatabaseExternalByarbejde> databaseExternalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndExternalByarbejde(SqlConnection, changeProviderId, externalByarbejdeId.Value);

			if (databaseExternalByarbejder.Any())
			{
				return;
			}

			DatabaseByarbejde databaseByarbejde = DatabaseByarbejde.ReadByNameOrCreate(SqlConnection, crmByarbejde.new_name);

			DatabaseExternalByarbejde databaseExternalByarbejde = new DatabaseExternalByarbejde(externalByarbejdeId.Value, changeProviderId, databaseByarbejde.Id);
			databaseExternalByarbejde.Insert(SqlConnection);
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

			DatabaseContactChange contactChange = CreateContactChange(changeProviderId, crmContact, externalContactId, contactId, modifiedOn);

			StoreContactRelations(crmContact, contactChange, changeProviderId);
		}

		internal void StoreInAccountChangesIfNeeded(Account crmAccount, Guid changeProviderId, Guid externalAccountId, DatabaseAccount account, DynamicsCrmConnection dynamicsCrmConnection)
		{
			Guid accountId = account.Id;
			DateTime modifiedOn = crmAccount.modifiedon;

			bool AccountChangeExists = DatabaseAccountChange.AccountChangeExists(SqlConnection, accountId, externalAccountId, changeProviderId, modifiedOn);

			if (AccountChangeExists == true)
			{
				return;
			}

			DatabaseAccountChange accountChange = CreateAccountChange(changeProviderId, crmAccount, externalAccountId, accountId, modifiedOn);

			StoreAccountRelations(crmAccount, accountChange, changeProviderId, dynamicsCrmConnection);
		}

		private DatabaseContact ReadOrCreateContact(Contact crmContact)
		{
			DatabaseContact contact = ContactCrmMapping.FindContact(Connection, SqlConnection, crmContact);

			if (contact == null)
			{
				contact = CreateContact(SqlConnection, crmContact);
			}

			return contact;
		}

		private DatabaseAccount ReadOrCreateAccount(Account crmAccount, Guid changeProviderId)
		{
			DatabaseAccount account = AccountCrmMapping.FindAccount(Connection, SqlConnection, crmAccount);

			if (account == null)
			{
				account = CreateAccount(SqlConnection, changeProviderId, crmAccount);
			}

			return account;
		}

		private DatabaseContact CreateContact(SqlConnection sqlConnection, Contact crmContact)
		{
			DatabaseContact contact = new DatabaseContact();

			Conversion.Contact.Convert(crmContact, contact);

			contact.Insert(sqlConnection);

			return contact;
		}

		private DatabaseAccount CreateAccount(SqlConnection sqlConnection, Guid changeProviderId, Account crmAccount)
		{
			DatabaseAccount account = new DatabaseAccount();

			Conversion.Account.Convert(sqlConnection, changeProviderId, crmAccount, account);

			account.Insert(sqlConnection);

			return account;
		}

		private DatabaseContactChange CreateContactChange(Guid changeProviderId, Contact crmContact, Guid externalContactId, Guid contactId, DateTime modifiedOn)
		{
			DatabaseContactChange contactChange = new DatabaseContactChange(SqlConnection, contactId, externalContactId, changeProviderId);

			Conversion.Contact.Convert(crmContact, contactChange);

			contactChange.Insert();

			return contactChange;
		}

		private void StoreContactRelations(Contact crmContact, DatabaseContactChange contactChange, Guid changeProviderId)
		{
			StoreContactRelationContactChangeGroup(crmContact, contactChange, changeProviderId);
			StoreContactRelationContactAnnotation(crmContact, contactChange, changeProviderId);
		}

		private void StoreContactRelationContactChangeGroup(Contact crmContact, DatabaseContactChange contactChange, Guid changeProviderId)
		{
			List<Group> externalGroupsFromContactGroup = crmContact.GetExternalGroupsFromContactGroup();
			List<DatabaseGroup> DatabaseGroups = externalGroupsFromContactGroup.Select(group => DatabaseGroup.ReadByNameOrCreate(SqlConnection, group.Name)).ToList();
			List<Guid> groupIds = DatabaseGroups.Select(group => group.Id).ToList();

			foreach (Guid groupId in groupIds)
			{
				DatabaseContactChangeGroup contactChangeGroup = new DatabaseContactChangeGroup(contactChange.Id, groupId);
				contactChangeGroup.Insert(SqlConnection);
			}
		}

		private void StoreContactRelationContactAnnotation(Contact crmContact, DatabaseContactChange contactChange, Guid changeProviderId)
		{
			List<Annotation> externalAnnotations = crmContact.GetAnnotations();

			externalAnnotations.ForEach(externalAnnotation =>
			{
				ExternalContactAnnotation externalContactAnnotation = ExternalContactAnnotation.ReadFromChangeProviderAndExternalAnnotation(SqlConnection, changeProviderId, externalAnnotation.Id).SingleOrDefault();

				Guid contactAnnotationId;

				if (externalContactAnnotation == null)
				{
					ContactAnnotation contactAnnotation = new ContactAnnotation(contactChange.ContactId);
					Conversion.Annotation.Convert(externalAnnotation, contactAnnotation);
					contactAnnotation.Insert(SqlConnection);

					contactAnnotationId = contactAnnotation.Id;

					externalContactAnnotation = new ExternalContactAnnotation(externalAnnotation.Id, changeProviderId, contactAnnotationId);
					externalContactAnnotation.Insert(SqlConnection);
				}
				else
				{
					contactAnnotationId = externalContactAnnotation.ContactAnnotationId;
				}

				ContactChangeAnnotation contactChangeAnnotation = new ContactChangeAnnotation(contactChange.Id, contactAnnotationId);
				Conversion.Annotation.Convert(externalAnnotation, contactChangeAnnotation);
				contactChangeAnnotation.Insert(SqlConnection);
			});

			List<ContactAnnotation> localContactAnnotations = ContactAnnotation.ReadByContactId(SqlConnection, contactChange.ContactId);

			List<Guid> remoteContactAnnotationIdsToKeep = externalAnnotations.
				Select(crmAnnotation => ExternalContactAnnotation.ReadFromChangeProviderAndExternalAnnotation(SqlConnection, changeProviderId, crmAnnotation.Id).SingleOrDefault()).
				Where(externalAnnotation => externalAnnotation != null).
				Select(externalAnnotation => externalAnnotation.ContactAnnotationId).ToList();

			List<ContactAnnotation> localContactAnnotationsToDelete = localContactAnnotations.
				Where(contactAnnotation => remoteContactAnnotationIdsToKeep.Any(keepId => contactAnnotation.Id == keepId) == false).ToList();

			localContactAnnotationsToDelete.ForEach(contactAnnotation =>
			{
				ContactChangeAnnotation contactChangeAnnotation = new ContactChangeAnnotation(contactChange.Id, contactAnnotation.Id);
				contactChangeAnnotation.modifiedon = contactChange.modifiedon;
				contactChangeAnnotation.isdeleted = true;
				contactChangeAnnotation.notetext = string.Empty;
				contactChangeAnnotation.Insert(SqlConnection);
			});
		}

		private DatabaseAccountChange CreateAccountChange(Guid changeProviderId, Account crmAccount, Guid externalAccountId, Guid accountId, DateTime modifiedOn)
		{
			DatabaseAccountChange accountChange = new DatabaseAccountChange(SqlConnection, accountId, externalAccountId, changeProviderId);

			Conversion.Account.Convert(SqlConnection, changeProviderId, crmAccount, accountChange);

			accountChange.Insert();

			return accountChange;
		}

		private void StoreAccountRelations(Account crmAccount, DatabaseAccountChange accountChange, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			StoreAccountRelationAccountChangeContact(crmAccount, accountChange, changeProviderId, dynamicsCrmConnection);
			StoreAccountRelationAccountChangeIndsamler(crmAccount, accountChange, changeProviderId, dynamicsCrmConnection);
			StoreAccountRelationAccountChangeGroup(crmAccount, accountChange, changeProviderId);
			StoreAccountRelationAccountAnnotation(crmAccount, accountChange, changeProviderId, dynamicsCrmConnection);
		}

		private void StoreAccountRelationAccountAnnotation(Account crmAccount, DatabaseAccountChange accountChange, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			List<Annotation> externalAnnotations = crmAccount.GetAnnotations();

			externalAnnotations.ForEach(externalAnnotation =>
			{
				ExternalAccountAnnotation externalAccountAnnotation = ExternalAccountAnnotation.ReadFromChangeProviderAndExternalAnnotation(SqlConnection, changeProviderId, externalAnnotation.Id).SingleOrDefault();

				Guid accountAnnotationId;

				if (externalAccountAnnotation == null)
				{
					AccountAnnotation accountAnnotation = new AccountAnnotation(accountChange.AccountId);
					Conversion.Annotation.Convert(externalAnnotation, accountAnnotation);
					accountAnnotation.Insert(SqlConnection);

					accountAnnotationId = accountAnnotation.Id;

					externalAccountAnnotation = new ExternalAccountAnnotation(externalAnnotation.Id, changeProviderId, accountAnnotationId);
					externalAccountAnnotation.Insert(SqlConnection);
				}
				else
				{
					accountAnnotationId = externalAccountAnnotation.AccountAnnotationId;
				}

				AccountChangeAnnotation accountChangeAnnotation = new AccountChangeAnnotation(accountChange.Id, accountAnnotationId);
				Conversion.Annotation.Convert(externalAnnotation, accountChangeAnnotation);
				accountChangeAnnotation.Insert(SqlConnection);
			});

			List<AccountAnnotation> localAccountAnnotations = AccountAnnotation.ReadByAccountId(SqlConnection, accountChange.AccountId);

			List<Guid> remoteAccountAnnotationIdsToKeep = externalAnnotations.
				Select(crmAnnotation => ExternalAccountAnnotation.ReadFromChangeProviderAndExternalAnnotation(SqlConnection, changeProviderId, crmAnnotation.Id).SingleOrDefault()).
				Where(externalAnnotation => externalAnnotation != null).
				Select(externalAnnotation => externalAnnotation.AccountAnnotationId).ToList();

			List<AccountAnnotation> localAccountAnnotationsToDelete = localAccountAnnotations.
				Where(accountAnnotation => remoteAccountAnnotationIdsToKeep.Any(keepId => accountAnnotation.Id == keepId) == false).ToList();

			localAccountAnnotationsToDelete.ForEach(accountAnnotation =>
			{
				AccountChangeAnnotation accountChangeAnnotation = new AccountChangeAnnotation(accountChange.Id, accountAnnotation.Id);
				accountChangeAnnotation.modifiedon = accountChange.modifiedon;
				accountChangeAnnotation.isdeleted = true;
				accountChangeAnnotation.notetext = string.Empty;
				accountChangeAnnotation.Insert(SqlConnection);
			});
		}

		private void StoreAccountRelationAccountChangeContact(Account crmAccount, DatabaseAccountChange accountChange, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			List<Guid> externalContactIdsFromAccountContact = crmAccount.GetExternalContactIdsFromAccountContact();

			externalContactIdsFromAccountContact = externalContactIdsFromAccountContact.Where(externalContactId => DatabaseExternalContact.Exists(SqlConnection, externalContactId, changeProviderId)).ToList();
			List<DatabaseExternalContact> externalContacts = externalContactIdsFromAccountContact.Select(externalContactId => ReadOrCreateContactFromExternalContactId(externalContactId, changeProviderId, dynamicsCrmConnection)).ToList();
			List<Guid> contactIds = externalContacts.Select(externalContact => externalContact.ContactId).ToList();

			foreach (Guid contactId in contactIds)
			{
				DatabaseAccountChangeContact accountChangeContact = new DatabaseAccountChangeContact(accountChange.Id, contactId);
				accountChangeContact.Insert(SqlConnection);
			}
		}

		private DatabaseExternalContact ReadOrCreateContactFromExternalContactId(Guid externalContactId, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			bool externalContactExists = DatabaseExternalContact.Exists(SqlConnection, externalContactId, changeProviderId);

			DatabaseExternalContact externalContact = null;
			if (externalContactExists)
			{
				externalContact = DatabaseExternalContact.Read(SqlConnection, externalContactId, changeProviderId);
				return externalContact;
			}

			Contact crmContact = Contact.Read(dynamicsCrmConnection, externalContactId);

			DatabaseContact databaseContact = new DatabaseContact();
			Conversion.Contact.Convert(crmContact, databaseContact);
			databaseContact.Insert(SqlConnection);

			externalContact = new DatabaseExternalContact(SqlConnection, externalContactId, changeProviderId, databaseContact.Id);

			externalContact.Insert();

			return externalContact;
		}

		private void StoreAccountRelationAccountChangeIndsamler(Account crmAccount, DatabaseAccountChange accountChange, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection)
		{
			foreach (IndsamlerDefinition definition in Account.IndsamlerRelationshipDefinitions)
			{
				StoreAccountRelationAccountChangeIndsamler(crmAccount, accountChange, changeProviderId, dynamicsCrmConnection, definition.IndsamlerType, definition.Aar);
			}
		}

		private void StoreAccountRelationAccountChangeIndsamler(Account crmAccount, DatabaseAccountChange accountChange, Guid changeProviderId, DynamicsCrmConnection dynamicsCrmConnection, IndsamlerDefinition.IndsamlerTypeEnum indsamlerType, int aar)
		{
			DatabaseAccountIndsamler.IndsamlerTypeEnum indsamlerTypeDatabase = Conversion.Account.Convert(indsamlerType);

			List<Guid> externalContactIdsFromAccountIndsamler = crmAccount.GetExternalContactIdsFromAccountIndsamler(indsamlerType, aar);
			List<DatabaseExternalContact> externalContacts = externalContactIdsFromAccountIndsamler.Select(externalContactId => ReadOrCreateContactFromExternalContactId(externalContactId, changeProviderId, dynamicsCrmConnection)).ToList();
			List<Guid> contactIds = externalContacts.Select(externalContact => externalContact.ContactId).ToList();

			foreach (Guid contactId in contactIds)
			{
				DatabaseAccountChangeIndsamler accountChangeIndsamler = new DatabaseAccountChangeIndsamler(accountChange.Id, contactId, indsamlerTypeDatabase, aar);
				accountChangeIndsamler.Insert(SqlConnection);
			}
		}

		private void StoreAccountRelationAccountChangeGroup(Account crmAccount, DatabaseAccountChange accountChange, Guid changeProviderId)
		{
			List<Group> externalGroupsFromAccountGroup = crmAccount.GetExternalGroupsFromAccountGroup();
			List<DatabaseGroup> DatabaseGroups = externalGroupsFromAccountGroup.Select(group => DatabaseGroup.ReadByNameOrCreate(SqlConnection, group.Name)).ToList();
			List<Guid> groupIds = DatabaseGroups.Select(group => group.Id).ToList();

			foreach (Guid groupId in groupIds)
			{
				DatabaseAccountChangeGroup contactChangeGroup = new DatabaseAccountChangeGroup(accountChange.Id, groupId);
				contactChangeGroup.Insert(SqlConnection);
			}
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
