using Administration.Option.Options.Logic;
using NUnit.Framework;
using System.Data.SqlClient;
using System;
using DatabaseSquash = DataLayer.MongoData.Option.Options.Logic.Squash;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseProgress = DataLayer.MongoData.Progress;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SquashTest : TestBase
	{
		private DatabaseChangeProvider _changeProvider1;
		private DatabaseChangeProvider _changeProvider2;

		private DatabaseExternalContact _externalContact1;
		private DatabaseExternalContact _externalContact2;

		private DatabaseExternalAccount _externalAccount1;
		private DatabaseExternalAccount _externalAccount2;

		private DatabaseContact _contact;

		private DatabaseAccount _account;

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

			_account = new DatabaseAccount()
			{
				name = "test",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};

			_contact.Insert(_sqlConnection);

			_account.Insert(_sqlConnection);

			string testSquashProvider1 = "testSquashProvider1";
			string testSquashProvider2 = "testSquashProvider2";

			_changeProvider1 = FindOrCreateChangeProvider(_sqlConnection, testSquashProvider1);
			_changeProvider2 = FindOrCreateChangeProvider(_sqlConnection, testSquashProvider2);

			DatabaseProgress progressContact = new DatabaseProgress()
			{
				LastProgressDate = DateTime.Now,
				TargetId = _contact.Id,
				TargetName = "Contact",
			};

			DatabaseProgress progressAccount = new DatabaseProgress()
			{
				LastProgressDate = DateTime.Now,
				TargetId = _account.Id,
				TargetName = "Account",
			};

			progressContact.Insert(Connection);

			progressAccount.Insert(Connection);

			_externalContact1 = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProvider1.Id);
			_externalContact1.Insert();

			_externalAccount1 = new DatabaseExternalAccount(_sqlConnection, Guid.NewGuid(), _changeProvider1.Id);
			_externalAccount1.Insert();

			_externalContact2 = new DatabaseExternalContact(_sqlConnection, Guid.NewGuid(), _changeProvider2.Id);
			_externalContact2.Insert();

			_externalAccount2 = new DatabaseExternalAccount(_sqlConnection, Guid.NewGuid(), _changeProvider2.Id);
			_externalAccount2.Insert();
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();

			_contact.Delete(_sqlConnection);

			_account.Delete(_sqlConnection);
		}

		[Test]
		public void ExecuteOptionSquashesToLatestSquashesContact()
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

		[Test]
		public void ExecuteOptionSquashesToLatestSquashesAccount()
		{
			DatabaseSquash databaseSquash = GetDatabaseSquash();

			Squash squash = new Squash(Connection, databaseSquash);

			CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 1), true);
			CreateAccountChange("name1", "name@2", new DateTime(2000, 1, 2), true);
			CreateAccountChange("name2", "name@2", new DateTime(2000, 1, 4), true);

			CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 3), false);

			squash.Execute();

			_account = DatabaseAccount.Read(_sqlConnection, _account.Id);

			Assert.AreEqual("name2", _account.name);
			Assert.AreEqual("name@1", _account.emailaddress1);
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

		private void CreateAccountChange(string name,string emailaddress1, DateTime modifiedOn, bool isProvider1)
		{
			Guid externalAccountId;
			Guid changeProviderId;

			if (isProvider1)
			{
				externalAccountId = _externalAccount1.ExternalAccountId;
				changeProviderId = _changeProvider1.Id;
			}
			else
			{
				externalAccountId = _externalAccount2.ExternalAccountId;
				changeProviderId = _changeProvider2.Id;
			}

			DatabaseAccountChange databaseAccountChange = new DatabaseAccountChange(_sqlConnection, _account.Id, externalAccountId, changeProviderId)
			{
				name = name,
				emailaddress1 = emailaddress1,
                ModifiedOn = modifiedOn,
				CreatedOn = DateTime.Now,
			};

			databaseAccountChange.Insert();
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
