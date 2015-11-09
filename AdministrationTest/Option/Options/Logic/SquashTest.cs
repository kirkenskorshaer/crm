using Administration.Option.Options.Logic;
using NUnit.Framework;
using System.Data.SqlClient;
using System;
using DatabaseSquash = DataLayer.MongoData.Option.Options.Logic.Squash;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseProgress = DataLayer.MongoData.Progress;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SquashTest : TestBase
	{
		private SqlConnection _sqlConnection;

		private DatabaseChangeProvider _changeProvider1;
		private DatabaseChangeProvider _changeProvider2;

		private DatabaseExternalContact _externalContact1;
		private DatabaseExternalContact _externalContact2;

		private DatabaseContact _contact;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_sqlConnection = DataLayer.SqlConnectionHolder.GetConnection(Connection, "sql");

			_contact = new DatabaseContact()
			{
				Firstname = "test",
				Lastname = "test",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};

			_contact.Insert(_sqlConnection);

			string testSquashProvider1 = "testSquashProvider1";
			string testSquashProvider2 = "testSquashProvider2";

			_changeProvider1 = FindOrCreateChangeProvider(_sqlConnection, testSquashProvider1);
			_changeProvider2 = FindOrCreateChangeProvider(_sqlConnection, testSquashProvider2);

			DatabaseProgress progress = new DatabaseProgress()
			{
				LastProgressDate = DateTime.Now,
				TargetId = _contact.Id,
				TargetName = "Contact",
			};

			progress.Insert(Connection);

			_externalContact1 = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProvider1.Id);
			_externalContact1.Insert();

			_externalContact2 = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProvider2.Id);
			_externalContact2.Insert();
		}

		[TearDown]
		public void TearDown()
		{
			_contact.Delete(_sqlConnection);
			_changeProvider1.Delete(_sqlConnection);
			_changeProvider2.Delete(_sqlConnection);
		}

		[Test]
		public void ExecuteOptionSquashesToLatest()
		{
			DatabaseSquash databaseSquash = GetDatabaseSquash();

			Squash squash = new Squash(Connection, databaseSquash);

			CreateContactChange("firstname1", "lastname1", new DateTime(2000, 1, 1), true);
			CreateContactChange("firstname1", "lastname2", new DateTime(2000, 1, 2), true);
			CreateContactChange("firstname2", "lastname2", new DateTime(2000, 1, 4), true);

			CreateContactChange("firstname1", "lastname1", new DateTime(2000, 1, 3), false);

			squash.Execute();

			_contact = DatabaseContact.Read(_sqlConnection, _contact.Id);

			Assert.AreEqual("firstname2", _contact.Firstname);
			Assert.AreEqual("lastname1", _contact.Lastname);
		}

		private void CreateContactChange(string firstName, string lastName, DateTime modifiedOn, bool isProvider1)
		{
			Guid externalContactId;
			Guid changeProviderId;

			if (isProvider1)
			{
				externalContactId = _externalContact1.ExternalContactId;
				changeProviderId = _changeProvider1.Id;
			}
			else
			{
				externalContactId = _externalContact2.ExternalContactId;
				changeProviderId = _changeProvider2.Id;
			}

			DatabaseContactChange databaseContactChange = new DatabaseContactChange(_sqlConnection, _contact.Id, externalContactId, changeProviderId)
			{
				Firstname = firstName,
				Lastname = lastName,
				ModifiedOn = modifiedOn,
				CreatedOn = DateTime.Now,
			};

			databaseContactChange.Insert();
		}

		private DatabaseSquash GetDatabaseSquash()
		{
			return new DatabaseSquash()
			{
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};
		}
	}
}
