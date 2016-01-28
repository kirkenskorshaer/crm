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

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SynchronizeToCrmTest : TestBase
	{
		private DataLayer.MongoData.UrlLogin _urlLogin;
		private DynamicsCrmConnection _dynamicsCrmConnection;
		private DatabaseChangeProvider _changeProvider;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_urlLogin = DataLayer.MongoData.UrlLogin.GetUrlLogin(Connection, "test");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(_urlLogin.Url, _urlLogin.Username, _urlLogin.Password);
			_changeProvider = FindOrCreateChangeProvider(_sqlConnection, "testCrmProvider");
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
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();

			MakeSureContactIsNextInProgressQueue(databaseContact);

			synchronizeToCrm.Execute();

			DatabaseExternalContact externalContact = DatabaseExternalContact.Read(_sqlConnection, _changeProvider.Id).Single();
			Contact contact = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);

			contact.Delete();
			Assert.AreEqual(databaseContact.Firstname, contact.firstname);
		}

		[Test]
		public void ExecuteOptionInsertsAccountInCrm()
		{
			DatabaseSynchronizeToCrm databaseSynchronizeToCrm = GetDatabaseSynchronizeToCrm();
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
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseContact databaseContact = CreateContact();

			MakeSureContactIsNextInProgressQueue(databaseContact);

			DatabaseExternalContact externalContact = new DatabaseExternalContact(_sqlConnection, contact.Id, _changeProvider.Id);
			externalContact.Insert();

			DatabaseContactChange contactChange = new DatabaseContactChange(_sqlConnection, databaseContact.Id, externalContact.ExternalContactId, _changeProvider.Id)
			{
				Firstname = "firstName2",
				Lastname = "lastname2",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};
			contactChange.Insert();

			synchronizeToCrm.Execute();

			Contact contactRead = Contact.Read(_dynamicsCrmConnection, externalContact.ExternalContactId);
			contact.Delete();

			Assert.AreEqual(contactChange.Firstname, contactRead.firstname);
		}

		[Test]
		public void ExecuteOptionUpdatesExistingAccountInCrm()
		{
			Account account = InsertCrmAccount();
			DatabaseSynchronizeToCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeToCrm();
			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeFromCrm);

			DatabaseAccount databaseAccount = CreateAccount();

			MakeSureAccountIsNextInProgressQueue(databaseAccount);

			DatabaseExternalAccount externalAccount = new DatabaseExternalAccount(_sqlConnection, account.Id, _changeProvider.Id);
			externalAccount.Insert();

			DatabaseAccountChange accountChange = new DatabaseAccountChange(_sqlConnection, databaseAccount.Id, externalAccount.ExternalAccountId, _changeProvider.Id)
			{
				name = "name2",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};
			accountChange.Insert();

			synchronizeToCrm.Execute();

			Account accountRead = Account.Read(_dynamicsCrmConnection, externalAccount.ExternalAccountId);
			account.Delete();

			Assert.AreEqual(accountChange.name, accountRead.name);
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
				Firstname = "firstName1",
				Lastname = "lastname1",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};

			contact.Insert(_sqlConnection);

			return contact;
		}

		private DatabaseAccount CreateAccount()
		{
			DatabaseAccount account = new DatabaseAccount
			{
				name = "name1",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};

			account.Insert(_sqlConnection);

			return account;
		}
	}
}
