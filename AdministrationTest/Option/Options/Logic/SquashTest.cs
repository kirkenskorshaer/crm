using Administration.Option.Options.Logic;
using NUnit.Framework;
using System.Data.SqlClient;
using System;
using DatabaseSquash = DataLayer.MongoData.Option.Options.Logic.Squash;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChangeGroup = DataLayer.SqlData.Group.ContactChangeGroup;
using DatabaseAccountChangeGroup = DataLayer.SqlData.Group.AccountChangeGroup;
using DatabaseAccountChangeContact = DataLayer.SqlData.Account.AccountChangeContact;
using DatabaseAccountChangeIndsamler = DataLayer.SqlData.Account.AccountChangeIndsamler;
using DatabaseGroup = DataLayer.SqlData.Group.Group;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseProgress = DataLayer.MongoData.Progress;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using System.Collections.Generic;
using System.Linq;
using DataLayer.SqlData.Account;

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
		public void ExecuteOptionSquashesToLatestSquashesContactGroup()
		{
			DatabaseSquash databaseSquash = GetDatabaseSquash();

			Squash squash = new Squash(Connection, databaseSquash);

			CreateContactChange("firstname1", "lastname1", new DateTime(2000, 1, 1), true, "groupA", "groupB", "groupC");
			CreateContactChange("firstname1", "lastname2", new DateTime(2000, 1, 2), true, "groupA", "groupB");
			CreateContactChange("firstname2", "lastname2", new DateTime(2000, 1, 4), true, "groupA", "groupB", "groupE");

			CreateContactChange("firstname1", "lastname1", new DateTime(2000, 1, 3), false, "groupB", "groupD");

			squash.Execute();

			List<string> groupsRead = DatabaseGroup.ReadGroupsFromContact(_sqlConnection, _contact.Id).Select(group => group.Name).ToList();
			groupsRead.Sort();
			List<string> expectedGroups = new List<string>() { "groupA", "groupB", "groupD", "groupE" };

			Assert.AreEqual(expectedGroups.Count, groupsRead.Count);
			for (int listIndex = 0; listIndex < expectedGroups.Count; listIndex++)
			{
				Assert.AreEqual(expectedGroups[listIndex], groupsRead[listIndex]);
			}
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

		[Test]
		public void ExecuteOptionSquashesToLatestSquashesAccountGroup()
		{
			DatabaseSquash databaseSquash = GetDatabaseSquash();

			Squash squash = new Squash(Connection, databaseSquash);

			DatabaseAccountChange databaseAccountChange1 = CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 1), true);
			AttachGroupToAccountChange(databaseAccountChange1, "groupA", "groupB", "groupC");

			DatabaseAccountChange databaseAccountChange2 = CreateAccountChange("name1", "name@2", new DateTime(2000, 1, 2), true);
			AttachGroupToAccountChange(databaseAccountChange2, "groupA", "groupB");

			DatabaseAccountChange databaseAccountChange3 = CreateAccountChange("name2", "name@2", new DateTime(2000, 1, 4), true);
			AttachGroupToAccountChange(databaseAccountChange3, "groupA", "groupB", "groupE");

			DatabaseAccountChange databaseAccountChange4 = CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 3), false);
			AttachGroupToAccountChange(databaseAccountChange4, "groupB", "groupD");

			squash.Execute();

			List<string> groupsRead = DatabaseGroup.ReadGroupsFromAccount(_sqlConnection, _account.Id).Select(group => group.Name).ToList();
			groupsRead.Sort();
			List<string> expectedGroups = new List<string>() { "groupA", "groupB", "groupD", "groupE" };

			Assert.AreEqual(expectedGroups.Count, groupsRead.Count);
			for (int listIndex = 0; listIndex < expectedGroups.Count; listIndex++)
			{
				Assert.AreEqual(expectedGroups[listIndex], groupsRead[listIndex]);
			}
		}

		[Test]
		public void ExecuteOptionSquashesToLatestSquashesAccountContact()
		{
			DatabaseSquash databaseSquash = GetDatabaseSquash();

			Squash squash = new Squash(Connection, databaseSquash);

			DatabaseAccountChange databaseAccountChange1 = CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 1), true);
			AttachContactToAccountChange(databaseAccountChange1, "firstnameA", "firstnameB", "firstnameC");

			DatabaseAccountChange databaseAccountChange2 = CreateAccountChange("name1", "name@2", new DateTime(2000, 1, 2), true);
			AttachContactToAccountChange(databaseAccountChange2, "firstnameA", "firstnameB");

			DatabaseAccountChange databaseAccountChange3 = CreateAccountChange("name2", "name@2", new DateTime(2000, 1, 4), true);
			AttachContactToAccountChange(databaseAccountChange3, "firstnameA", "firstnameB", "firstnameE");

			DatabaseAccountChange databaseAccountChange4 = CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 3), false);
			AttachContactToAccountChange(databaseAccountChange4, "firstnameB", "firstnameD");

			squash.Execute();

			List<string> contactsRead = DatabaseContact.ReadContactsFromAccountContact(_sqlConnection, _account.Id).Select(contact => contact.Firstname).ToList();
			contactsRead.Sort();
			List<string> expectedNames = new List<string>() { "firstnameA", "firstnameB", "firstnameD", "firstnameE" };

			Assert.AreEqual(expectedNames.Count, contactsRead.Count);
			for (int listIndex = 0; listIndex < expectedNames.Count; listIndex++)
			{
				Assert.AreEqual(expectedNames[listIndex], contactsRead[listIndex]);
			}
		}

		[Test]
		public void ExecuteOptionSquashesToLatestSquashesAccountIndsamler()
		{
			DatabaseSquash databaseSquash = GetDatabaseSquash();

			Squash squash = new Squash(Connection, databaseSquash);

			DatabaseAccountChange databaseAccountChange1 = CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 1), true);
			AttachIndsamlerToAccountChange(databaseAccountChange1, "firstnameA", "firstnameB", "firstnameC");

			DatabaseAccountChange databaseAccountChange2 = CreateAccountChange("name1", "name@2", new DateTime(2000, 1, 2), true);
			AttachIndsamlerToAccountChange(databaseAccountChange2, "firstnameA", "firstnameB");

			DatabaseAccountChange databaseAccountChange3 = CreateAccountChange("name2", "name@2", new DateTime(2000, 1, 4), true);
			AttachIndsamlerToAccountChange(databaseAccountChange3, "firstnameA", "firstnameB", "firstnameE");

			DatabaseAccountChange databaseAccountChange4 = CreateAccountChange("name1", "name@1", new DateTime(2000, 1, 3), false);
			AttachIndsamlerToAccountChange(databaseAccountChange4, "firstnameB", "firstnameD");

			squash.Execute();

			List<string> contactsRead = DatabaseContact.ReadContactsFromAccountIndsamler(_sqlConnection, _account.Id).Select(contact => contact.Firstname).ToList();
			contactsRead.Sort();
			List<string> expectedNames = new List<string>() { "firstnameA", "firstnameB", "firstnameD", "firstnameE" };

			Assert.AreEqual(expectedNames.Count, contactsRead.Count);
			for (int listIndex = 0; listIndex < expectedNames.Count; listIndex++)
			{
				Assert.AreEqual(expectedNames[listIndex], contactsRead[listIndex]);
			}
		}

		private void AttachGroupToAccountChange(DatabaseAccountChange databaseAccountChange, params string[] groups)
		{
			foreach (string groupname in groups)
			{
				DatabaseGroup group = ReadOrCreateGroup(groupname);

				DatabaseAccountChangeGroup accountChangeGroup = new DatabaseAccountChangeGroup(databaseAccountChange.Id, group.Id);
				accountChangeGroup.Insert(_sqlConnection);
			}
		}

		private void AttachContactToAccountChange(DatabaseAccountChange databaseAccountChange, params string[] contactFirstnames)
		{
			foreach (string contactFirstname in contactFirstnames)
			{
				DatabaseContact contact = ReadOrCreateContact(contactFirstname);

				DatabaseAccountChangeContact accountChangeContact = new DatabaseAccountChangeContact(databaseAccountChange.Id, contact.Id);
				accountChangeContact.Insert(_sqlConnection);
			}
		}

		private void AttachIndsamlerToAccountChange(DatabaseAccountChange databaseAccountChange, params string[] contactFirstnames)
		{
			foreach (string contactFirstname in contactFirstnames)
			{
				DatabaseContact contact = ReadOrCreateContact(contactFirstname);

				DatabaseAccountChangeIndsamler accountChangeIndsamler = new DatabaseAccountChangeIndsamler(databaseAccountChange.Id, contact.Id);
				accountChangeIndsamler.Insert(_sqlConnection);
			}
		}

		private DatabaseContact ReadOrCreateContact(string contactFirstname)
		{
			List<DatabaseContact> existingContacts = DatabaseContact.Read(_sqlConnection, contactFirstname);

			DatabaseContact contact;
			if (existingContacts.Any())
			{
				contact = existingContacts.First();
			}
			else
			{
				contact = new DatabaseContact()
				{
					Firstname = contactFirstname,
					Lastname = "test",
					ModifiedOn = DateTime.Now,
					CreatedOn = DateTime.Now,
				};

				contact.Insert(_sqlConnection);
			}

			return contact;
		}

		private void CreateContactChange(string firstName, string lastName, DateTime modifiedOn, bool isProvider1, params string[] groups)
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

			foreach (string groupname in groups)
			{
				DatabaseGroup group = ReadOrCreateGroup(groupname);

				DatabaseContactChangeGroup contactGroup = new DatabaseContactChangeGroup(databaseContactChange.Id, group.Id);
				contactGroup.Insert(_sqlConnection);
			}
		}

		private DatabaseGroup ReadOrCreateGroup(string groupname)
		{
			DatabaseGroup group;
			if (DatabaseGroup.ExistsByName(_sqlConnection, groupname))
			{
				group = DatabaseGroup.ReadByName(_sqlConnection, groupname);
			}
			else
			{
				group = new DatabaseGroup()
				{
					Name = groupname,
				};
				group.Insert(_sqlConnection);
			}
			return group;
		}

		private DatabaseAccountChange CreateAccountChange(string name, string emailaddress1, DateTime modifiedOn, bool isProvider1)
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

			return databaseAccountChange;
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
