using Administration.Option.Options.Logic.Campaign;
using NUnit.Framework;
using DatabaseSynchronizeFromStub = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromStub;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseStub = DataLayer.MongoData.Input.Stub;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
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

			_databaseWebCampaign = InsertDatabaseWebCampaign();
			_databaseSynchronizeFromStub = InsertDatabaseSynchronizeFromStub(_databaseWebCampaign);
			_synchronizeFromStub = new SynchronizeFromStub(Connection, _databaseSynchronizeFromStub);
			_squash = new Squash(Connection, null);
			_changeProvider = FindOrCreateChangeProvider(_sqlConnection, $"WebCampaign {_databaseWebCampaign.FormId}");
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

			DatabaseWebCampaign wrongDatabaseWebCampaign = InsertDatabaseWebCampaign();
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

			DatabaseStub stub = InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));

			_synchronizeFromStub.Execute();

			stub = DatabaseStub.ReadFirst(Connection);

			Assert.IsNull(stub);
		}

		[Test]
		public void stubWillMergeOnKeyField()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			string firstname2 = $"firstname {Guid.NewGuid()}";
			string lastname = $"lastname {Guid.NewGuid()}";

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

			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname, new DateTime(2000, 1, 1));
			InsertDatabaseStub(_databaseWebCampaign, firstname, lastname2, new DateTime(2000, 1, 2));

			_synchronizeFromStub.Execute();
			_synchronizeFromStub.Execute();

			int numberOfContact = GetSquashedContactsOnChangeProvider().Count();

			Assert.AreEqual(2, numberOfContact);
		}

		private List<DatabaseContact> GetSquashedContactsOnChangeProvider()
		{
			List<DatabaseContactChange> contactChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<Guid> contactIds = contactChanges.Select(contactChange => contactChange.ContactId).Distinct().ToList();

			List<DatabaseContact> contacts = contactIds.Select(contactId => DatabaseContact.Read(_sqlConnection, contactId)).ToList();
			contacts.ForEach(contact => _squash.SquashContact(contact));

			return contacts;
		}

		private DatabaseStub InsertDatabaseStub(DatabaseWebCampaign databaseWebCampaign, string firstname, string lastname, DateTime postDate)
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

			stub.Push(Connection);

			return stub;
		}

		private DatabaseSynchronizeFromStub InsertDatabaseSynchronizeFromStub(DatabaseWebCampaign databaseWebCampaign)
		{
			DatabaseSynchronizeFromStub databaseSynchronizeFromStub = DatabaseSynchronizeFromStub.Create(Connection, databaseWebCampaign._id, "test", CreateScheduleAlwaysOnDoOnce());

			return databaseSynchronizeFromStub;
		}

		private DatabaseWebCampaign InsertDatabaseWebCampaign()
		{
			DatabaseWebCampaign databaseWebCampaign = new DatabaseWebCampaign()
			{
				FormId = Guid.NewGuid(),
				KeyField = "lastname",
				RedirectTarget = "test.html",
			};
			databaseWebCampaign.Insert(Connection);

			return databaseWebCampaign;
		}
	}
}
