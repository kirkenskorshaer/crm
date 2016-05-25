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
using DatabaseByarbejde = DataLayer.SqlData.Byarbejde.Byarbejde;
using DatabaseExternalByarbejde = DataLayer.SqlData.Byarbejde.ExternalByarbejde;
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
using DataLayer.SqlData.Contact;
using DataLayer.SqlData.Annotation;

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

			if (_databaseSynchronizeToCrm.synchronizeType.HasFlag(DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact))
			{
				SynchronizeContacts(changeProviderId);
			}

			if (_databaseSynchronizeToCrm.synchronizeType.HasFlag(DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account))
			{
				SynchronizeAccounts(changeProviderId);
			}

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
				externalAccounts.ForEach(account => UpdateExternalAccountIfNeeded(changeProviderId, account, databaseAccount, _dynamicsCrmConnection));
			}

			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private void InsertContactAndCreateExternalContact(Guid changeProviderId, DatabaseContact databaseContact)
		{
			SystemInterfaceContact systemInterfaceContact = Conversion.Contact.Convert(_dynamicsCrmConnection, databaseContact);
			systemInterfaceContact.Insert();

			DatabaseExternalContact externalContact = new DatabaseExternalContact(SqlConnection, systemInterfaceContact.Id, changeProviderId, databaseContact.Id);
			externalContact.Insert();

			InsertContactRelations(databaseContact, systemInterfaceContact, changeProviderId);
		}

		private void InsertContactRelations(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId)
		{
			InsertAccountContact(databaseContact, systemInterfaceContact, changeProviderId);
			InsertContactGroup(databaseContact, systemInterfaceContact);
			InsertContactAnnotation(databaseContact, systemInterfaceContact, changeProviderId);
			InsertAccountIndsamler(databaseContact, systemInterfaceContact, changeProviderId);
		}

		private void InsertAccountAndCreateExternalAccount(Guid changeProviderId, DatabaseAccount databaseAccount)
		{
			InsertByarbejdeIfNeeded(changeProviderId, databaseAccount);

			SystemInterfaceAccount systemInterfaceAccount = Conversion.Account.Convert(_dynamicsCrmConnection, SqlConnection, changeProviderId, databaseAccount);
			systemInterfaceAccount.Insert();

			DatabaseExternalAccount externalAccount = new DatabaseExternalAccount(SqlConnection, systemInterfaceAccount.Id, changeProviderId, databaseAccount.Id);
			externalAccount.Insert();

			InsertAccountRelations(databaseAccount, systemInterfaceAccount, changeProviderId);
		}

		private void InsertByarbejdeIfNeeded(Guid changeProviderId, DatabaseAccount databaseAccount)
		{
			if (databaseAccount.byarbejdeid.HasValue == false)
			{
				return;
			}

			List<DatabaseExternalByarbejde> databaseExternalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndByarbejde(SqlConnection, changeProviderId, databaseAccount.byarbejdeid.Value);

			if (databaseExternalByarbejder.Any())
			{
				return;
			}

			DatabaseByarbejde databaseByarbejde = DatabaseByarbejde.Read(SqlConnection, databaseAccount.byarbejdeid.Value);

			Byarbejde crmByarbejde = new Byarbejde(_dynamicsCrmConnection);
			Conversion.Byarbejde.Convert(databaseByarbejde, crmByarbejde);
			crmByarbejde.Insert();

			DatabaseExternalByarbejde databaseExternalByarbejde = new DatabaseExternalByarbejde(crmByarbejde.Id, changeProviderId, databaseByarbejde.Id);
			databaseExternalByarbejde.Insert(SqlConnection);
		}

		private void InsertAccountRelations(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId)
		{
			InsertAccountContact(databaseAccount, systemInterfaceAccount, changeProviderId);
			InsertAccountIndsamler(databaseAccount, systemInterfaceAccount, changeProviderId);
			InsertAccountGroup(databaseAccount, systemInterfaceAccount);
			InsertAccountAnnotation(databaseAccount, systemInterfaceAccount, changeProviderId);
		}

		private void UpdateExternalContactIfNeeded(Guid changeProviderId, DatabaseExternalContact databaseExternalContact, DatabaseContact databaseContact)
		{
			bool isDeactivated = false;

			SystemInterfaceContact systemInterfaceContactInCrm = SystemInterfaceContact.Read(_dynamicsCrmConnection, databaseExternalContact.ExternalContactId);

			isDeactivated = UpdateExternalContactData(changeProviderId, databaseExternalContact, databaseContact, systemInterfaceContactInCrm);

			isDeactivated = SynchronizeContactGroup(databaseContact, systemInterfaceContactInCrm, changeProviderId, isDeactivated);
			isDeactivated = SynchronizeAccountContact(databaseContact, systemInterfaceContactInCrm, changeProviderId, isDeactivated);
			isDeactivated = SynchronizeAccountIndsamler(databaseContact, systemInterfaceContactInCrm, changeProviderId, isDeactivated);
			isDeactivated = SynchronizeContactAnnotation(databaseContact, systemInterfaceContactInCrm, changeProviderId, isDeactivated);

			if (isDeactivated)
			{
				systemInterfaceContactInCrm.SetActive(true);
			}
		}

		private bool UpdateExternalContactData(Guid changeProviderId, DatabaseExternalContact databaseExternalContact, DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContactInCrm)
		{
			SystemInterfaceContact systemInterfaceContact = Conversion.Contact.Convert(_dynamicsCrmConnection, databaseContact);

			if (systemInterfaceContactInCrm.Equals(systemInterfaceContact))
			{
				return false;
			}

			systemInterfaceContactInCrm.SetActive(false);

			systemInterfaceContactInCrm = SystemInterfaceContact.Read(_dynamicsCrmConnection, databaseExternalContact.ExternalContactId);

			_synchronizeFromCrm.StoreInContactChangesIfNeeded(systemInterfaceContactInCrm, changeProviderId, databaseExternalContact.ExternalContactId, databaseContact);

			databaseContact = DatabaseContact.Read(SqlConnection, databaseContact.Id);

			_squash.SquashContact(databaseContact);

			Conversion.Contact.Convert(databaseContact, systemInterfaceContactInCrm);

			systemInterfaceContactInCrm.Update();

			return true;
		}

		private void UpdateExternalAccountIfNeeded(Guid changeProviderId, DatabaseExternalAccount databaseExternalAccount, DatabaseAccount databaseAccount, DynamicsCrmConnection dynamicsCrmConnection)
		{
			bool isDeactivated = false;

			SystemInterfaceAccount systemInterfaceAccountInCrm = SystemInterfaceAccount.Read(_dynamicsCrmConnection, databaseExternalAccount.ExternalAccountId);

			isDeactivated = UpdateExternalAccountData(changeProviderId, databaseExternalAccount, databaseAccount, systemInterfaceAccountInCrm, isDeactivated, dynamicsCrmConnection);

			isDeactivated = SynchronizeAccountContact(databaseAccount, systemInterfaceAccountInCrm, changeProviderId, isDeactivated);
			isDeactivated = SynchronizeAccountIndsamler(databaseAccount, systemInterfaceAccountInCrm, changeProviderId, isDeactivated);
			isDeactivated = SynchronizeAccountGroup(databaseAccount, systemInterfaceAccountInCrm, isDeactivated);
			isDeactivated = SynchronizeAccountAnnotation(databaseAccount, systemInterfaceAccountInCrm, changeProviderId, isDeactivated);

			if (isDeactivated)
			{
				systemInterfaceAccountInCrm.SetActive(true);
			}
		}

		private bool UpdateExternalAccountData(Guid changeProviderId, DatabaseExternalAccount databaseExternalAccount, DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccountInCrm, bool isDeactivated, DynamicsCrmConnection dynamicsCrmConnection)
		{
			SystemInterfaceAccount systemInterfaceAccount = Conversion.Account.Convert(_dynamicsCrmConnection, SqlConnection, changeProviderId, databaseAccount);

			if (systemInterfaceAccountInCrm.Equals(systemInterfaceAccount))
			{
				return isDeactivated;
			}

			systemInterfaceAccountInCrm.SetActive(false);

			systemInterfaceAccountInCrm = SystemInterfaceAccount.Read(_dynamicsCrmConnection, databaseExternalAccount.ExternalAccountId);

			_synchronizeFromCrm.StoreInAccountChangesIfNeeded(systemInterfaceAccountInCrm, changeProviderId, databaseExternalAccount.ExternalAccountId, databaseAccount, dynamicsCrmConnection);

			databaseAccount = DatabaseAccount.Read(SqlConnection, databaseAccount.Id);

			_squash.SquashAccount(databaseAccount);

			Conversion.Account.Convert(SqlConnection, changeProviderId, databaseAccount, systemInterfaceAccountInCrm);

			systemInterfaceAccountInCrm.Update();

			return true;
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

		private bool SynchronizeAccountContact(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId, bool isDeactivated)
		{
			List<Guid> systemInterfaceAccountsAlreadyAssociated = systemInterfaceContact.GetExternalAccountIdsFromAccountContact();

			List<DatabaseAccountContact> accountContacts = DatabaseAccountContact.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseAccount> databaseAccounts = accountContacts.Select(accountContact => DatabaseAccount.Read(SqlConnection, accountContact.AccountId)).ToList();

			List<Guid> accountIds = GetExternalAccountIdsFromDatabaseAccounts(changeProviderId, databaseAccounts);

			if (ListCompare.ListEquals(systemInterfaceAccountsAlreadyAssociated, accountIds))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceContact.SetActive(false);
				_squash.SquashContact(databaseContact);
			}

			systemInterfaceContact.SynchronizeAccounts(accountIds);

			return true;
		}

		private bool InsertAccountContact(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId)
		{
			List<DatabaseAccountContact> accountContacts = DatabaseAccountContact.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseAccount> databaseAccounts = accountContacts.Select(accountContact => DatabaseAccount.Read(SqlConnection, accountContact.AccountId)).ToList();

			List<Guid> accountIds = GetExternalAccountIdsFromDatabaseAccounts(changeProviderId, databaseAccounts);

			systemInterfaceContact.SynchronizeAccounts(accountIds);

			return true;
		}

		private bool SynchronizeContactGroup(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId, bool isDeactivated)
		{
			List<SystemInterfaceGroup> systemInterfaceGroupsAlreadyAssociated = systemInterfaceContact.GetExternalGroupsFromContactGroup();

			List<DatabaseContactGroup> contactGroups = DatabaseContactGroup.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseGroup> databaseGroups = contactGroups.Select(contactGroup => DatabaseGroup.Read(SqlConnection, contactGroup.GroupId)).ToList();

			List<string> groupNames = databaseGroups.Select(group => group.Name).ToList();

			if (ListCompare.ListEquals(systemInterfaceGroupsAlreadyAssociated, groupNames, (interfaceGroup, groupName) => interfaceGroup.Name == groupName))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceContact.SetActive(false);
				_squash.SquashContact(databaseContact);
			}

			systemInterfaceContact.SynchronizeGroups(groupNames);

			return true;
		}

		private bool SynchronizeContactAnnotation(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId, bool isDeactivated)
		{
			List<Annotation> crmAnnotations = systemInterfaceContact.GetAnnotations();

			List<ContactAnnotation> localContactAnnotations = ContactAnnotation.ReadByContactIdAndIsdeleted(SqlConnection, databaseContact.Id, false);

			if (ListCompare.ListEquals(crmAnnotations, localContactAnnotations, (crmAnnotation, contactAnnotation) => crmAnnotation.notetext == contactAnnotation.notetext))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceContact.SetActive(false);
				_squash.SquashContact(databaseContact);
			}

			List<Annotation> annotationsToSynchronize = new List<Annotation>();
			localContactAnnotations.ForEach(localContactAnnotation =>
			{
				ExternalContactAnnotation externalContactAnnotation = ExternalContactAnnotation.ReadFromChangeProviderAndAnnotation(SqlConnection, changeProviderId, localContactAnnotation.Id).SingleOrDefault();

				if (externalContactAnnotation == null)
				{
					Annotation annotation = new Annotation(_dynamicsCrmConnection);
					Conversion.Annotation.Convert(localContactAnnotation, annotation);

					annotation.Insert();
					annotationsToSynchronize.Add(annotation);

					externalContactAnnotation = new ExternalContactAnnotation(annotation.Id, changeProviderId, localContactAnnotation.Id);
					externalContactAnnotation.Insert(SqlConnection);
				}
				else
				{
					Annotation annotation = crmAnnotations.Single(crmAnnotation => crmAnnotation.Id == externalContactAnnotation.ExternalAnnotationId);
					Conversion.Annotation.Convert(localContactAnnotation, annotation);
					annotation.Update();

					annotationsToSynchronize.Add(annotation);
				}
			});

			systemInterfaceContact.SynchronizeAnnotations(annotationsToSynchronize);

			return true;
		}

		private bool SynchronizeAccountAnnotation(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId, bool isDeactivated)
		{
			List<Annotation> crmAnnotations = systemInterfaceAccount.GetAnnotations();

			List<AccountAnnotation> localAccountAnnotations = AccountAnnotation.ReadByAccountIdAndIsdeleted(SqlConnection, databaseAccount.Id, false);

			if (ListCompare.ListEquals(crmAnnotations, localAccountAnnotations, (crmAnnotation, accountAnnotation) => crmAnnotation.notetext == accountAnnotation.notetext))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceAccount.SetActive(false);
				_squash.SquashAccount(databaseAccount);
			}

			List<Annotation> annotationsToSynchronize = new List<Annotation>();
			localAccountAnnotations.ForEach(localAccountAnnotation =>
			{
				ExternalAccountAnnotation externalAccountAnnotation = ExternalAccountAnnotation.ReadFromChangeProviderAndAnnotation(SqlConnection, changeProviderId, localAccountAnnotation.Id).SingleOrDefault();

				if (externalAccountAnnotation == null)
				{
					Annotation annotation = new Annotation(_dynamicsCrmConnection);
					Conversion.Annotation.Convert(localAccountAnnotation, annotation);

					annotation.Insert();
					annotationsToSynchronize.Add(annotation);

					externalAccountAnnotation = new ExternalAccountAnnotation(annotation.Id, changeProviderId, localAccountAnnotation.Id);
					externalAccountAnnotation.Insert(SqlConnection);
				}
				else
				{
					Annotation annotation = crmAnnotations.Single(crmAnnotation => crmAnnotation.Id == externalAccountAnnotation.ExternalAnnotationId);
					Conversion.Annotation.Convert(localAccountAnnotation, annotation);
					annotation.Update();

					annotationsToSynchronize.Add(annotation);
				}
			});

			systemInterfaceAccount.SynchronizeAnnotations(annotationsToSynchronize);

			return true;
		}

		private void InsertContactGroup(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact)
		{
			List<DatabaseContactGroup> contactGroups = DatabaseContactGroup.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseGroup> databaseGroups = contactGroups.Select(contactGroup => DatabaseGroup.Read(SqlConnection, contactGroup.GroupId)).ToList();

			List<string> groupNames = databaseGroups.Select(group => group.Name).ToList();
			systemInterfaceContact.SynchronizeGroups(groupNames);
		}

		private void InsertContactAnnotation(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId)
		{
			List<ContactAnnotation> contactAnnotations = ContactAnnotation.ReadByContactIdAndIsdeleted(SqlConnection, databaseContact.Id, false);

			List<Annotation> crmAnnotations = new List<Annotation>();
			contactAnnotations.ForEach(contactAnnotation =>
			{
				Annotation crmAnnotation = new Annotation(_dynamicsCrmConnection);
				Conversion.Annotation.Convert(contactAnnotation, crmAnnotation);

				crmAnnotation.Insert();
				crmAnnotations.Add(crmAnnotation);

				ExternalContactAnnotation externalContactAnnotation = new ExternalContactAnnotation(crmAnnotation.Id, changeProviderId, contactAnnotation.Id);
				externalContactAnnotation.Insert(SqlConnection);
			});

			systemInterfaceContact.SynchronizeAnnotations(crmAnnotations);
		}

		private void InsertAccountAnnotation(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId)
		{
			List<AccountAnnotation> accountAnnotations = AccountAnnotation.ReadByAccountIdAndIsdeleted(SqlConnection, databaseAccount.Id, false);

			List<Annotation> crmAnnotations = new List<Annotation>();
			accountAnnotations.ForEach(accountAnnotation =>
			{
				Annotation crmAnnotation = new Annotation(_dynamicsCrmConnection);
				Conversion.Annotation.Convert(accountAnnotation, crmAnnotation);

				crmAnnotation.Insert();
				crmAnnotations.Add(crmAnnotation);

				ExternalAccountAnnotation externalAccountAnnotation = new ExternalAccountAnnotation(crmAnnotation.Id, changeProviderId, accountAnnotation.Id);
				externalAccountAnnotation.Insert(SqlConnection);
			});

			systemInterfaceAccount.SynchronizeAnnotations(crmAnnotations);
		}

		private bool SynchronizeAccountIndsamler(DatabaseContact databaseContact, SystemInterfaceContact systemInterfaceContact, Guid changeProviderId, bool isDeactivated)
		{
			List<Guid> systemInterfaceAccountsAlreadyAssociated = systemInterfaceContact.GetExternalAccountIdsFromAccountIndsamlere();

			List<DatabaseAccountIndsamler> accountIndsamlere = DatabaseAccountIndsamler.ReadFromContactId(SqlConnection, databaseContact.Id);

			List<DatabaseAccount> databaseAccounts = accountIndsamlere.Select(accountContact => DatabaseAccount.Read(SqlConnection, accountContact.AccountId)).ToList();

			List<Guid> accountIds = GetExternalAccountIdsFromDatabaseAccounts(changeProviderId, databaseAccounts);

			if (ListCompare.ListEquals(systemInterfaceAccountsAlreadyAssociated, accountIds))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceContact.SetActive(false);
				_squash.SquashContact(databaseContact);
			}

			systemInterfaceContact.SynchronizeIndsamlere(accountIds);

			return true;
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

		private bool SynchronizeAccountContact(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId, bool isDeactivated)
		{
			List<Guid> systemInterfaceContactsAlreadyAssociated = systemInterfaceAccount.GetExternalContactIdsFromAccountContact();

			List<DatabaseAccountContact> accountContacts = DatabaseAccountContact.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseContact> databaseContacts = accountContacts.Select(accountContact => DatabaseContact.Read(SqlConnection, accountContact.ContactId)).ToList();

			List<Guid> contactIds = GetExternalContactIdsFromDatabaseContacts(changeProviderId, databaseContacts);

			if (ListCompare.ListEquals(contactIds, systemInterfaceContactsAlreadyAssociated))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceAccount.SetActive(false);
				_squash.SquashAccount(databaseAccount);
			}

			systemInterfaceAccount.SynchronizeContacts(contactIds);

			return true;
		}

		private void InsertAccountContact(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId)
		{
			List<DatabaseAccountContact> accountContacts = DatabaseAccountContact.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseContact> databaseContacts = accountContacts.Select(accountContact => DatabaseContact.Read(SqlConnection, accountContact.ContactId)).ToList();

			List<Guid> contactIds = GetExternalContactIdsFromDatabaseContacts(changeProviderId, databaseContacts);

			systemInterfaceAccount.SynchronizeContacts(contactIds);
		}

		private bool SynchronizeAccountIndsamler(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId, bool isDeactivated, DatabaseAccountIndsamler.IndsamlerTypeEnum indsamlerType, int aar)
		{
			IndsamlerDefinition.IndsamlerTypeEnum indsamlerTypeCrm = Conversion.Account.Convert(indsamlerType);

			List<Guid> systemInterfaceContactsAlreadyAssociated = systemInterfaceAccount.GetExternalContactIdsFromAccountIndsamler(indsamlerTypeCrm, aar);

			List<DatabaseAccountIndsamler> accountIndsamlere = DatabaseAccountIndsamler.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseContact> databaseContacts = accountIndsamlere.Select(accountContact => DatabaseContact.Read(SqlConnection, accountContact.ContactId)).ToList();

			List<Guid> contactIds = GetExternalContactIdsFromDatabaseContacts(changeProviderId, databaseContacts);

			if (ListCompare.ListEquals(contactIds, systemInterfaceContactsAlreadyAssociated))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceAccount.SetActive(false);
				_squash.SquashAccount(databaseAccount);
			}

			systemInterfaceAccount.SynchronizeIndsamlere(contactIds, aar, indsamlerTypeCrm);

			return true;
		}

		private bool SynchronizeAccountIndsamler(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId, bool isDeactivated)
		{
			foreach (IndsamlerDefinition definition in SystemInterfaceAccount.IndsamlerRelationshipDefinitions)
			{
				DatabaseAccountIndsamler.IndsamlerTypeEnum indsamlerTypeDatabase = Conversion.Account.Convert(definition.IndsamlerType);
				isDeactivated = SynchronizeAccountIndsamler(databaseAccount, systemInterfaceAccount, changeProviderId, isDeactivated, indsamlerTypeDatabase, definition.Aar);
			}
			return isDeactivated;
		}

		private void InsertAccountIndsamler(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId)
		{
			foreach (IndsamlerDefinition definition in SystemInterfaceAccount.IndsamlerRelationshipDefinitions)
			{
				InsertAccountIndsamler(databaseAccount, systemInterfaceAccount, changeProviderId, definition.IndsamlerType, definition.Aar);
			}
		}

		private void InsertAccountIndsamler(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, Guid changeProviderId, IndsamlerDefinition.IndsamlerTypeEnum indsamlerType, int aar)
		{
			List<DatabaseAccountIndsamler> accountIndsamlere = DatabaseAccountIndsamler.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseContact> databaseContacts = accountIndsamlere.Select(accountContact => DatabaseContact.Read(SqlConnection, accountContact.ContactId)).ToList();

			List<Guid> contactIds = GetExternalContactIdsFromDatabaseContacts(changeProviderId, databaseContacts);

			systemInterfaceAccount.SynchronizeIndsamlere(contactIds, aar, indsamlerType);
		}

		private bool SynchronizeAccountGroup(DatabaseAccount databaseAccount, SystemInterfaceAccount systemInterfaceAccount, bool isDeactivated)
		{
			List<Guid> systemInterfaceGroupsAlreadyAssociated = systemInterfaceAccount.GetExternalGroupsFromAccountGroup().Select(group => group.GroupId).ToList();

			List<DatabaseAccountGroup> accountGroups = DatabaseAccountGroup.ReadFromAccountId(SqlConnection, databaseAccount.Id);

			List<DatabaseGroup> databaseGroups = accountGroups.Select(accountGroup => DatabaseGroup.Read(SqlConnection, accountGroup.GroupId)).ToList();

			List<SystemInterfaceGroup> systemInterfaceGroups = databaseGroups.Select(databaseGroup => SystemInterfaceGroup.ReadOrCreate(_dynamicsCrmConnection, databaseGroup.Name)).ToList();

			List<Guid> groupIds = systemInterfaceGroups.Select(group => group.GroupId).ToList();

			if (ListCompare.ListEquals(groupIds, systemInterfaceGroupsAlreadyAssociated))
			{
				return isDeactivated;
			}

			if (isDeactivated == false)
			{
				systemInterfaceAccount.SetActive(false);
				_squash.SquashAccount(databaseAccount);
			}

			systemInterfaceAccount.SynchronizeGroups(groupIds);

			return true;
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
