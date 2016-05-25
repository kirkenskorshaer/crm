using Administration.Option.Options.Logic.Campaign;
using NUnit.Framework;
using DatabaseSynchronizeFromStub = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromStub;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseStub = DataLayer.MongoData.Input.Stub;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseAccountIndsamler = DataLayer.SqlData.Account.AccountIndsamler;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using System;
using DataLayer.MongoData.Input;
using Administration.Option.Options.Logic;
using System.Linq;
using System.Collections.Generic;

namespace AdministrationTest.Option.Options.Logic.Campaign
{
	[TestFixture]
	public class SynchronizeFromStubTest : TestBase
	{
		private DatabaseWebCampaign _databaseWebCampaign;
		private DatabaseSynchronizeFromStub _databaseSynchronizeFromStub;
		private SynchronizeFromStub _synchronizeFromStub;
		private Squash _squash;
		private DatabaseChangeProvider _changeProvider;

		[SetUp]
		public new void SetUp()
		{
			base.SetUp();

			_squash = new Squash(Connection, null);
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();

			_changeProvider.Delete(_sqlConnection);
		}

		[Test]
		public void stubCanBeSynchronized()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Contact, "lastname");
			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));

			_synchronizeFromStub.Execute();

			DatabaseContact contact = GetSquashedContactsOnChangeProvider().Single();

			Assert.AreEqual(firstname, contact.firstname);
		}

		[Test]
		public void stubWillBeSynchronizedOnTheRightCampaign()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Contact, "lastname");
			DatabaseWebCampaign wrongDatabaseWebCampaign = InsertDatabaseWebCampaign(DatabaseWebCampaign.CollectTypeEnum.Contact, "lastname");
			InsertDatabaseStub(wrongDatabaseWebCampaign, $"firstname {Guid.NewGuid()}", $"lastname {Guid.NewGuid()}", new DateTime(2000, 1, 1));
			InsertDatabaseStub(wrongDatabaseWebCampaign, $"firstname {Guid.NewGuid()}", $"lastname {Guid.NewGuid()}", new DateTime(2000, 1, 1));

			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));

			InsertDatabaseStub(wrongDatabaseWebCampaign, $"firstname {Guid.NewGuid()}", $"lastname {Guid.NewGuid()}", new DateTime(2000, 1, 1));
			InsertDatabaseStub(wrongDatabaseWebCampaign, $"firstname {Guid.NewGuid()}", $"lastname {Guid.NewGuid()}", new DateTime(2000, 1, 1));

			_synchronizeFromStub.Execute();

			DatabaseContact contact = GetSquashedContactsOnChangeProvider().Single();

			Assert.AreEqual(firstname, contact.firstname);
		}

		[Test]
		public void stubWillBeRemovedAfterSynchronize()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Contact, "lastname");

			DatabaseStub stub = InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));

			_synchronizeFromStub.Execute();

			stub = DatabaseStub.ReadFirst(Connection);

			Assert.IsNull(stub);
		}

		[Test]
		public void stubWillBeRequeuedAfterFailedSynchronize()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Contact, "wrongKey");

			DatabaseStub stub1 = InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));
			_synchronizeFromStub.Execute();
			DatabaseStub stub2 = InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 2));

			DatabaseStub stubRead = DatabaseStub.ReadFirst(Connection, _databaseWebCampaign);
			Assert.AreEqual(stub2._id, stubRead._id);
			Assert.AreEqual(0, stubRead.ImportAttempt);

			_synchronizeFromStub.Execute();
			stubRead = DatabaseStub.ReadFirst(Connection, _databaseWebCampaign);
			Assert.AreEqual(stub1._id, stubRead._id);
			Assert.AreEqual(1, stubRead.ImportAttempt);
		}

		[Test]
		public void stubWillMergeOnKeyField()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string firstname2 = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Contact, "lastname");

			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));
			InsertDatabaseStub(_databaseWebCampaign, firstname2, lastname, new DateTime(2000, 1, 2));

			_synchronizeFromStub.Execute();
			_synchronizeFromStub.Execute();

			DatabaseContact contact = GetSquashedContactsOnChangeProvider().Single();

			Assert.AreEqual(firstname2, contact.firstname);
		}

		[Test]
		public void stubWillNotBeMergeOnNonKeyField()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";
			string lastname2 = $"lastname {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Contact, "lastname");

			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));
			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname2, new DateTime(2000, 1, 2));

			_synchronizeFromStub.Execute();
			_synchronizeFromStub.Execute();

			int numberOfContact = GetSquashedContactsOnChangeProvider().Count();

			Assert.AreEqual(2, numberOfContact);
		}

		[Test]
		public void stubCanRelateToAccount()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";
			string accountName = $"name {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.ContactWithAccountRelation, "lastname");

			DatabaseAccount accountCreated = CreateAccount(_changeProvider.Id, accountName, new DateTime(2000, 1, 1));

			StubElement indsamlingsstedStubElement = new StubElement() { Key = "indsamlingssted2016", Value = accountCreated.Id.ToString() };
			DatabaseStub stub = InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2001, 1, 1), indsamlingsstedStubElement);

			_synchronizeFromStub.Execute();

			DatabaseContact contact = GetSquashedContactsOnChangeProvider().Single();
			GetSquashedAccountsOnChangeProvider();
			DatabaseAccountIndsamler accountIndsamler = DatabaseAccountIndsamler.ReadFromContactId(_sqlConnection, contact.Id).Single();

			Assert.AreEqual(accountCreated.Id, accountIndsamler.AccountId);
		}

		[Test]
		public void stubCanSynchronizeAccount()
		{
			string name = $"name {Guid.NewGuid()}";

			ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum.Account, "name");

			StubElement nameStubElement = new StubElement() { Key = "name", Value = name };
			InsertDatabaseStub(_databaseWebCampaign, "firstname", "lastname", new DateTime(2000, 1, 1), nameStubElement);

			_synchronizeFromStub.Execute();

			DatabaseAccount account = GetSquashedAccountsOnChangeProvider().Single();

			Assert.AreEqual(name, account.name);
		}

		private void ArrangeCampaignParameters(DatabaseWebCampaign.CollectTypeEnum collectType, string keyField)
		{
			_databaseWebCampaign = InsertDatabaseWebCampaign(collectType, keyField);
			_databaseSynchronizeFromStub = InsertDatabaseSynchronizeFromStub(_databaseWebCampaign);
			_synchronizeFromStub = new SynchronizeFromStub(Connection, _databaseSynchronizeFromStub);
			_changeProvider = FindOrCreateChangeProvider(_sqlConnection, $"WebCampaign {_databaseWebCampaign.FormId}");
		}

		private List<DatabaseContact> GetSquashedContactsOnChangeProvider()
		{
			List<DatabaseContactChange> contactChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<Guid> contactIds = contactChanges.Select(contactChange => contactChange.ContactId).Distinct().ToList();

			List<DatabaseContact> contacts = contactIds.Select(contactId => DatabaseContact.Read(_sqlConnection, contactId)).ToList();
			contacts.ForEach(contact => _squash.SquashContact(contact));

			return contacts;
		}

		private List<DatabaseAccount> GetSquashedAccountsOnChangeProvider()
		{
			List<DatabaseAccountChange> accountChanges = DatabaseAccountChange.Read(_sqlConnection, _changeProvider.Id, DatabaseAccountChange.IdType.ChangeProviderId);
			List<Guid> accountIds = accountChanges.Select(accountChange => accountChange.AccountId).Distinct().ToList();

			List<DatabaseAccount> accounts = accountIds.Select(accountId => DatabaseAccount.Read(_sqlConnection, accountId)).ToList();
			accounts.ForEach(account => _squash.SquashAccount(account));

			return accounts;
		}

		private DatabaseStub InsertDatabaseStub(DatabaseWebCampaign databaseWebCampaign, string firstname, string lastname, DateTime postDate, params StubElement[] otherStubElements)
		{
			DatabaseStub stub = new DatabaseStub()
			{
				WebCampaignId = databaseWebCampaign._id,
				PostTime = postDate,
				Contents = new List<StubElement>()
				{
					new StubElement() {Key = "firstname", Value = firstname},
					new StubElement() {Key = "lastname", Value = lastname},
				},
			};

			stub.Contents.AddRange(otherStubElements);

			stub.Push(Connection);

			return stub;
		}

		private DatabaseSynchronizeFromStub InsertDatabaseSynchronizeFromStub(DatabaseWebCampaign databaseWebCampaign)
		{
			DatabaseSynchronizeFromStub databaseSynchronizeFromStub = DatabaseSynchronizeFromStub.Create(Connection, databaseWebCampaign._id, "test", CreateScheduleAlwaysOnDoOnce());

			return databaseSynchronizeFromStub;
		}

		private DatabaseWebCampaign InsertDatabaseWebCampaign(DatabaseWebCampaign.CollectTypeEnum collectType, string keyField)
		{
			DatabaseWebCampaign databaseWebCampaign = new DatabaseWebCampaign()
			{
				FormId = Guid.NewGuid(),
				KeyField = keyField,
				RedirectTarget = "test.html",
				CollectType = collectType,
			};
			databaseWebCampaign.Insert(Connection);

			return databaseWebCampaign;
		}
	}
}
