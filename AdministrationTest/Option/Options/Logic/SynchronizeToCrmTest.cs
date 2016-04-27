using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseSynchronizeToCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DatabaseByarbejde = DataLayer.SqlData.Byarbejde.Byarbejde;
using DatabaseExternalByarbejde = DataLayer.SqlData.Byarbejde.ExternalByarbejde;
using DatabaseGroup = DataLayer.SqlData.Group.Group;
using DatabaseContactGroup = DataLayer.SqlData.Group.ContactGroup;
using DatabaseContactChangeGroup = DataLayer.SqlData.Group.ContactChangeGroup;
using DatabaseAccountGroup = DataLayer.SqlData.Group.AccountGroup;
using DatabaseAccountContact = DataLayer.SqlData.Account.AccountContact;
using DatabaseAccountIndsamler = DataLayer.SqlData.Account.AccountIndsamler;
using System.Collections.Generic;
using DataLayer.SqlData.Annotation;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SynchronizeToCrmTest : TestBase
	{
		private DataLayer.MongoData.UrlLogin _urlLogin;
		private DynamicsCrmConnection _dynamicsCrmConnection;
		private DatabaseChangeProvider _changeProvider;
		private DatabaseChangeProvider _changeProviderLocal;
		private Squash _squash;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_urlLogin = DataLayer.MongoData.UrlLogin.GetUrlLogin(Connection, "test");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(_urlLogin.Url, _urlLogin.Username, _urlLogin.Password);
			_changeProvider = FindOrCreateChangeProvider(_sqlConnection, "testCrmProvider");
			_changeProviderLocal = FindOrCreateChangeProvider(_sqlConnection, "testCrmProviderLocal");
			_squash = new Squash(Connection, null);
		}

		private DatabaseSynchronizeToCrm GetDatabaseSynchronizeToCrm()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = new DatabaseSynchronizeToCrm
			{
				changeProviderId = _changeProvider.Id,
				Name = "SynchronizeToCrmTest",
				urlLoginName = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			return databaseSynchronizeFromCrm;
		}

		[Test]
		public void ExecuteOptionInsertsContactInCrm()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();

			MakeSureContactIsNextInProgressQueue(databaseContact);

			synchronizeToCrm.Execute();

			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Contact contact = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);

			contact.Delete();
			Assert.AreEqual(databaseContact.firstname, contact.firstname);
		}

		[Test]
		public void ExecuteOptionInsertsAccountInCrm()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeToCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			DatabaseAccount databaseAccount = CreateAccount();

			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			synchronizeToCrm.Execute();

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.Read(_sqlConnection, _changeProvider.Id).Single();
			Account account = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);

			account.Delete();
			Assert.AreEqual(databaseAccount.name, account.name);
		}

		[Test]
		public void ExecuteOptionUpdatesExistingContactInCrm()
		{
			Contact contact = InsertCrmContact();
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();

			MakeSureContactIsNextInProgressQueue(databaseContact);

			DatabaseExternalContact externalContact = new DatabaseExternalContact(_sqlConnection, contact.Id, _changeProvider.Id, databaseContact.Id);
			externalContact.Insert();

			DatabaseContactChange contactChange = new DatabaseContactChange(_sqlConnection, databaseContact.Id, externalContact.ExternalContactId, _changeProvider.Id)
			{
				firstname = "firstName2",
				lastname = "lastname2",
				modifiedon = DateTime.Now,
				createdon = DateTime.Now,
			};
			contactChange.Insert();

			synchronizeToCrm.Execute();

			Contact contactRead = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);
			contact.Delete();

			Assert.AreEqual(contactChange.firstname, contactRead.firstname);
		}

		[Test]
		public void ExecuteOptionUpdatesExistingAccountInCrm()
		{
			Account account = InsertCrmAccount();
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseAccount databaseAccount = CreateAccount();

			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			DatabaseExternalAccount externalAccount = new DatabaseExternalAccount(_sqlConnection, account.Id, _changeProvider.Id, databaseAccount.Id);
			externalAccount.Insert();

			DatabaseAccountChange accountChange = new DatabaseAccountChange(_sqlConnection, databaseAccount.Id, externalAccount.ExternalAccountId, _changeProvider.Id)
			{
				name = "name2",
				modifiedon = DateTime.Now,
				createdon = DateTime.Now,
			};
			accountChange.Insert();

			synchronizeToCrm.Execute();

			Account accountRead = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);
			account.Delete();

			Assert.AreEqual(accountChange.name, accountRead.name);
		}

		[Test]
		public void GroupsWillBeSynchronizedOnContact()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();
			DatabaseExternalContact databaseExternalContact = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProviderLocal.Id, databaseContact.Id);
			databaseExternalContact.Insert();
			AddGroupsToDatabaseContact(databaseContact, databaseExternalContact, "group1", "group2");

			MakeSureContactIsNextInProgressQueue(databaseContact);

			synchronizeToCrm.Execute();

			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Contact contact = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);
			List<string> readGroupNames = contact.Groups.Select(group => group.Name).ToList();
			readGroupNames.Sort();

			contact.Delete();
			Assert.AreEqual("group1", readGroupNames.First());
			Assert.AreEqual("group2", readGroupNames.Last());
		}

		[Test]
		public void GroupsCanBeRemovedFromContact()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();
			DatabaseExternalContact databaseExternalContact = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProviderLocal.Id, databaseContact.Id);
			databaseExternalContact.Insert();
			AddGroupsToDatabaseContact(databaseContact, databaseExternalContact, "group1", "group2");

			MakeSureContactIsNextInProgressQueue(databaseContact);

			synchronizeToCrm.Execute();

			AddGroupsToDatabaseContact(databaseContact, databaseExternalContact, "group2");

			synchronizeToCrm.Execute();

			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Contact contact = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);
			List<string> readGroupNames = contact.Groups.Select(group => group.Name).ToList();

			contact.Delete();
			Assert.AreEqual("group2", readGroupNames.Single());
		}

		[Test]
		public void GroupsWillBeSynchronizedOnAccount()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseAccount databaseAccount = CreateAccount();
			AddGroupsToDatabaseAccount(databaseAccount, "group1", "group2");

			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			synchronizeToCrm.Execute();

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.Read(_sqlConnection, _changeProvider.Id).Single();
			Account account = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);
			List<string> readGroupNames = account.ReadGroups().Select(group => group.Name).ToList();

			readGroupNames.Sort();

			account.Delete();
			Assert.AreEqual("group1", readGroupNames.First());
			Assert.AreEqual("group2", readGroupNames.Last());
		}

		[Test]
		public void AccountsWillBeSynchronized()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Both;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();
			MakeSureContactIsNextInProgressQueue(databaseContact);

			DatabaseAccount databaseAccount = CreateAccount();
			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			DatabaseAccountContact accountContact = new DatabaseAccountContact(databaseAccount.Id, databaseContact.Id);
			accountContact.Insert(_sqlConnection);

			synchronizeToCrm.Execute();

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.Read(_sqlConnection, _changeProvider.Id).Single();
			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Account account = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);
			List<Contact> contacts = account.ReadContacts();

			Assert.AreEqual(externalContact.ExternalContactId, contacts.Single().Id);
		}

		[Test]
		public void IndsamlereWillBeSynchronized()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeFromCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Both;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();
			MakeSureContactIsNextInProgressQueue(databaseContact);

			DatabaseAccount databaseAccount = CreateAccount();
			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			DatabaseAccountIndsamler accountIndsamler = new DatabaseAccountIndsamler(databaseAccount.Id, databaseContact.Id);
			accountIndsamler.Insert(_sqlConnection);

			synchronizeToCrm.Execute();

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.Read(_sqlConnection, _changeProvider.Id).Single();
			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Account account = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);
			List<Contact> indsamlere = account.ReadIndsamlere();

			Assert.AreEqual(externalContact.ExternalContactId, indsamlere.Single().Id);
		}

		[Test]
		public void ByarbejdeWillBeSynchronized()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeToCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			DatabaseAccount databaseAccount = CreateAccount();
			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			DatabaseByarbejde databaseByarbejde = new DatabaseByarbejde()
			{
				new_name = $"name {Guid.NewGuid()}",
			};
			databaseByarbejde.Insert(_sqlConnection);

			databaseAccount.byarbejdeid = databaseByarbejde.Id;
			databaseAccount.Update(_sqlConnection);

			synchronizeToCrm.Execute();

			Byarbejde byarbejde = Byarbejde.Read(_dynamicsCrmConnection, databaseByarbejde.new_name).Single();

			Assert.AreEqual(databaseByarbejde.new_name, byarbejde.new_name);
		}

		[Test]
		public void AnnotationOnContactCanBeAdded()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeToCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			DatabaseContact databaseContact = CreateContact();
			MakeSureContactIsNextInProgressQueue(databaseContact);

			ContactAnnotation contactAnnotation = CreateContactAnnotation(databaseContact);

			synchronizeToCrm.Execute();

			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Contact contact = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);

			List<Annotation> annotationsRead = contact.GetAnnotations();

			contact.Delete();

			Assert.AreEqual(annotationsRead.Single().notetext, contactAnnotation.notetext);
		}

		[Test]
		public void AnnotationOnContactCanBeRemoved()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeToCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Contact;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			DatabaseContact databaseContact = CreateContact();
			MakeSureContactIsNextInProgressQueue(databaseContact);

			ContactAnnotation contactAnnotation1 = CreateContactAnnotation(databaseContact);
			ContactAnnotation contactAnnotation2 = CreateContactAnnotation(databaseContact);
			ContactAnnotation contactAnnotation3 = CreateContactAnnotation(databaseContact);

			synchronizeToCrm.Execute();

			DeleteContactAnnotation(contactAnnotation2, databaseContact);

			synchronizeToCrm.Execute();

			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Contact contact = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);

			List<Annotation> annotationsRead = contact.GetAnnotations();

			contact.Delete();

			Assert.AreEqual(2, annotationsRead.Count);
			Assert.IsTrue(annotationsRead.Any(annotation => annotation.notetext == contactAnnotation1.notetext));
			Assert.IsTrue(annotationsRead.Any(annotation => annotation.notetext == contactAnnotation3.notetext));
		}

		[Test]
		public void AnnotationOnAccountCanBeAdded()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeToCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			DatabaseAccount databaseAccount = CreateAccount();
			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			AccountAnnotation accountAnnotation = CreateAccountAnnotation(databaseAccount);

			synchronizeToCrm.Execute();

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.Read(_sqlConnection, _changeProvider.Id).Single();
			Account account = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);

			List<Annotation> annotationsRead = account.GetAnnotations();

			account.Delete();

			Assert.AreEqual(annotationsRead.Single().notetext, accountAnnotation.notetext);
		}

		[Test]
		public void AnnotationOnAccountCanBeRemoved()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
			databaseSynchronizeToCrm.synchronizeType = DatabaseSynchronizeToCrm.SynchronizeTypeEnum.Account;
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			DatabaseAccount databaseAccount = CreateAccount();
			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			AccountAnnotation accountAnnotation1 = CreateAccountAnnotation(databaseAccount);
			AccountAnnotation accountAnnotation2 = CreateAccountAnnotation(databaseAccount);
			AccountAnnotation accountAnnotation3 = CreateAccountAnnotation(databaseAccount);

			synchronizeToCrm.Execute();

			DeleteAccountAnnotation(accountAnnotation2, databaseAccount);

			synchronizeToCrm.Execute();

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.Read(_sqlConnection, _changeProvider.Id).Single();
			Account account = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);

			List<Annotation> annotationsRead = account.GetAnnotations();

			account.Delete();

			Assert.AreEqual(2, annotationsRead.Count);
			Assert.IsTrue(annotationsRead.Any(annotation => annotation.notetext == accountAnnotation1.notetext));
			Assert.IsTrue(annotationsRead.Any(annotation => annotation.notetext == accountAnnotation3.notetext));
		}

		private ContactAnnotation CreateContactAnnotation(DatabaseContact databaseContact)
		{
			ContactAnnotation contactAnnotation = new ContactAnnotation(databaseContact.Id);
			contactAnnotation.notetext = $"note {Guid.NewGuid()}";
			contactAnnotation.modifiedon = DateTime.Now;
			contactAnnotation.Insert(_sqlConnection);
			return contactAnnotation;
		}

		private void DeleteContactAnnotation(ContactAnnotation contactAnnotation2, DatabaseContact databaseContact)
		{
			DatabaseExternalContact externalContact = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProviderLocal.Id, databaseContact.Id);
			externalContact.Insert();

			DatabaseContactChange change = new DatabaseContactChange(_sqlConnection, databaseContact.Id, externalContact.ExternalContactId, _changeProviderLocal.Id);
			change.modifiedon = DateTime.Now;
			change.createdon = DateTime.Now;
			change.Insert();

			ContactChangeAnnotation changeAnnotation = new ContactChangeAnnotation(change.Id, contactAnnotation2.Id);
			changeAnnotation.modifiedon = change.modifiedon;
			changeAnnotation.isdeleted = true;
			changeAnnotation.notetext = string.Empty;
			changeAnnotation.Insert(_sqlConnection);
		}

		private AccountAnnotation CreateAccountAnnotation(DatabaseAccount databaseAccount)
		{
			AccountAnnotation accountAnnotation = new AccountAnnotation(databaseAccount.Id);
			accountAnnotation.notetext = $"note {Guid.NewGuid()}";
			accountAnnotation.modifiedon = DateTime.Now;
			accountAnnotation.Insert(_sqlConnection);
			return accountAnnotation;
		}

		private void DeleteAccountAnnotation(AccountAnnotation accountAnnotation2, DatabaseAccount databaseAccount)
		{
			DatabaseExternalAccount externalAccount = new DatabaseExternalAccount(_sqlConnection, Guid.NewGuid(), _changeProviderLocal.Id, databaseAccount.Id);
			externalAccount.Insert();

			DatabaseAccountChange change = new DatabaseAccountChange(_sqlConnection, databaseAccount.Id, externalAccount.ExternalAccountId, _changeProviderLocal.Id);
			change.modifiedon = DateTime.Now;
			change.createdon = DateTime.Now;
			change.Insert();

			AccountChangeAnnotation changeAnnotation = new AccountChangeAnnotation(change.Id, accountAnnotation2.Id);
			changeAnnotation.modifiedon = change.modifiedon;
			changeAnnotation.isdeleted = true;
			changeAnnotation.notetext = string.Empty;
			changeAnnotation.Insert(_sqlConnection);
		}

		private void AddGroupsToDatabaseContact(DatabaseContact databaseContact, DatabaseExternalContact databaseExternalContact, params string[] groupNames)
		{
			DatabaseContactChange databaseContactChange = new DatabaseContactChange(_sqlConnection, databaseContact.Id, databaseExternalContact.ExternalContactId, _changeProviderLocal.Id)
			{
				createdon = DateTime.Now,
				modifiedon = DateTime.Now,
			};

			databaseContactChange.Insert();
			foreach (string groupName in groupNames)
			{
				DatabaseGroup group = ReadOrCreateGroup(groupName);
				DatabaseContactChangeGroup contactChangeGroup = new DatabaseContactChangeGroup(databaseContactChange.Id, group.Id);
				contactChangeGroup.Insert(_sqlConnection);
			}

			_squash.SquashContact(databaseContact);
		}

		private void AddGroupsToDatabaseAccount(DatabaseAccount databaseAccount, params string[] groupNames)
		{
			foreach (string groupName in groupNames)
			{
				DatabaseGroup group = ReadOrCreateGroup(groupName);

				DatabaseAccountGroup accountGroup = new DatabaseAccountGroup(databaseAccount.Id, group.Id);
				accountGroup.Insert(_sqlConnection);
			}
		}

		private DatabaseGroup ReadOrCreateGroup(string groupName)
		{
			if (DatabaseGroup.ExistsByName(_sqlConnection, groupName))
			{
				return DatabaseGroup.ReadByName(_sqlConnection, groupName);
			}

			DatabaseGroup group = new DatabaseGroup()
			{
				Name = groupName,
			};

			group.Insert(_sqlConnection);

			return group;
		}

		private Contact InsertCrmContact()
		{
			Contact contact = new Contact(_dynamicsCrmConnection)
			{
				firstname = "firstName1",
				lastname = "lastname1",
				modifiedon = DateTime.Now,
				createdon = DateTime.Now,
			};

			contact.Insert();

			return contact;
		}

		private Account InsertCrmAccount()
		{
			Account account = new Account(_dynamicsCrmConnection)
			{
				name = "firstName1",
			};

			account.Insert();

			return account;
		}

		private void MakeSureContactIsNextInProgressQueue(DatabaseContact databaseContact)
		{
			DataLayer.MongoData.Progress progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressContactToCrm);
			if (progress == null)
			{
				progress = new DataLayer.MongoData.Progress()
				{
					LastProgressDate = DateTime.Now,
					TargetName = MaintainProgress.ProgressContactToCrm,
				};
				progress.Insert(Connection);
			}
			progress.TargetId = databaseContact.Id;
			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private void MakeSureAccountIsNextInProgressQueue(DatabaseAccount databaseAccount)
		{
			DataLayer.MongoData.Progress progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressAccountToCrm);
			if (progress == null)
			{
				progress = new DataLayer.MongoData.Progress()
				{
					LastProgressDate = DateTime.Now,
					TargetName = MaintainProgress.ProgressAccountToCrm,
				};
				progress.Insert(Connection);
			}
			progress.TargetId = databaseAccount.Id;
			progress.UpdateAndSetLastProgressDateToNow(Connection);
		}

		private DatabaseContact CreateContact()
		{
			DatabaseContact contact = new DatabaseContact
			{
				firstname = "firstName1",
				lastname = "lastname1",
				modifiedon = DateTime.Now,
				createdon = DateTime.Now,
			};

			contact.Insert(_sqlConnection);

			return contact;
		}

		private DatabaseAccount CreateAccount()
		{
			DatabaseAccount account = new DatabaseAccount
			{
				name = "name1",
				modifiedon = DateTime.Now,
				createdon = DateTime.Now,
			};

			account.Insert(_sqlConnection);

			return account;
		}
	}
}
