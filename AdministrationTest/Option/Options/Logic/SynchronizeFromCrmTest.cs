using NUnit.Framework;
using SystemInterface.Dynamics.Crm;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseSynchronizeFromCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCrm;
using DatabaseAccountChangeContact = DataLayer.SqlData.Account.AccountChangeContact;
using DatabaseAccountChangeIndsamler = DataLayer.SqlData.Account.AccountChangeIndsamler;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseContactChangeGroup = DataLayer.SqlData.Group.ContactChangeGroup;
using DatabaseAccountChangeGroup = DataLayer.SqlData.Group.AccountChangeGroup;
using DatabaseGroup = DataLayer.SqlData.Group.Group;
using System.Linq;
using System.Collections.Generic;
using Administration.Option.Options.Logic;
using System;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SynchronizeFromCrmTest : TestBase
	{
		private DataLayer.MongoData.UrlLogin _urlLogin;
		private DynamicsCrmConnection _dynamicsCrmConnection;
		private DatabaseChangeProvider _changeProvider;
		private SynchronizeFromCrm _synchronizeFromCrm;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_urlLogin = DataLayer.MongoData.UrlLogin.GetUrlLogin(Connection, "test");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(_urlLogin.Url, _urlLogin.Username, _urlLogin.Password);

			_changeProvider = FindOrCreateChangeProvider(_sqlConnection, "testCrmProvider");

			DatabaseSynchronizeFromCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeFromCrm();
			_synchronizeFromCrm = new SynchronizeFromCrm(Connection, databaseSynchronizeFromCrm);
			_synchronizeFromCrm.Execute();
		}

		private DatabaseSynchronizeFromCrm GetDatabaseSynchronizeFromCrm()
		{
			DatabaseSynchronizeFromCrm databaseSynchronizeFromCrm = new DatabaseSynchronizeFromCrm
			{
				changeProviderId = _changeProvider.Id,
				Name = "SynchronizeFromCrmTest",
				urlLoginName = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			return databaseSynchronizeFromCrm;
		}

		[Test]
		public void ExecuteOptionGetsChangesets()
		{
			string firstname1 = "firstname1";
			string firstname2 = "firstname2";

			Contact crmContact = CreateCrmContact(firstname1);

			crmContact.Insert();
			_synchronizeFromCrm.Execute();

			crmContact.firstname = firstname2;
			crmContact.Update();
			_synchronizeFromCrm.Execute();

			List<DatabaseContactChange> contactChanges = DatabaseContactChange.Read(_sqlConnection, crmContact.Id, DatabaseContactChange.IdType.ExternalContactId);

			crmContact.Delete();

			Assert.AreEqual(2, contactChanges.Count);
			Assert.IsTrue(contactChanges.Any(contactChange => contactChange.firstname == firstname1));
			Assert.IsTrue(contactChanges.Any(contactChange => contactChange.firstname == firstname2));
		}

		[Test]
		public void ContactGroupsCanBeAdded()
		{
			string firstname = "firstname";
			string groupName = "groupName";

			Contact crmContact = CreateCrmContact(firstname);

			crmContact.Insert();

			crmContact.SynchronizeGroups(new List<string>() { groupName });

			_synchronizeFromCrm.Execute();

			DatabaseExternalContact databaseExternalContact = DatabaseExternalContact.Read(_sqlConnection, crmContact.Id, _changeProvider.Id);
			DatabaseContactChange databaseContactchange = DatabaseContactChange.Read(_sqlConnection, databaseExternalContact.ContactId, DatabaseContactChange.IdType.ContactId).
				Where(contactChange => contactChange.ChangeProviderId == _changeProvider.Id && contactChange.ExternalContactId == databaseExternalContact.ExternalContactId).Single();

			List<DatabaseContactChangeGroup> databaseContactGroups = DatabaseContactChangeGroup.ReadFromContactChangeId(_sqlConnection, databaseContactchange.Id);
			List<DatabaseGroup> databaseGroups = databaseContactGroups.Select(contactGroup => DatabaseGroup.Read(_sqlConnection, contactGroup.GroupId)).ToList();

			crmContact.Delete();

			Assert.AreEqual(groupName, databaseGroups.Single().Name);
		}

		[Test]
		public void AccountContactsCanBeAdded()
		{
			string firstname1 = "firstname1";
			string name2 = "name2";

			Contact crmContact = CreateCrmContact(firstname1);
			Account crmAccount = CreateCrmAccount(name2);

			crmContact.Insert();
			crmAccount.Insert();

			crmAccount.SynchronizeContacts(new List<Contact>() { crmContact });

			_synchronizeFromCrm.Execute();

			DatabaseExternalContact databaseExternalContact = DatabaseExternalContact.Read(_sqlConnection, crmContact.Id, _changeProvider.Id);
			List<DatabaseAccountChangeContact> accountChangeContacts = DatabaseAccountChangeContact.ReadFromContactId(_sqlConnection, databaseExternalContact.ContactId);

			crmContact.Delete();
			crmAccount.Delete();

			Assert.AreEqual(1, accountChangeContacts.Count);
		}

		[Test]
		public void AccountIndsamlereCanBeAdded()
		{
			string firstname1 = "firstname1";
			string name2 = "name2";

			Contact crmContact = CreateCrmContact(firstname1);
			Account crmAccount = CreateCrmAccount(name2);

			crmContact.Insert();
			crmAccount.Insert();

			crmAccount.SynchronizeIndsamlere(new List<Contact>() { crmContact });

			_synchronizeFromCrm.Execute();

			DatabaseExternalContact databaseExternalContact = DatabaseExternalContact.Read(_sqlConnection, crmContact.Id, _changeProvider.Id);
			List<DatabaseAccountChangeIndsamler> accountChangeIndsamlere = DatabaseAccountChangeIndsamler.ReadFromContactId(_sqlConnection, databaseExternalContact.ContactId);

			crmContact.Delete();
			crmAccount.Delete();

			Assert.AreEqual(1, accountChangeIndsamlere.Count);
		}

		[Test]
		public void AccountGroupsCanBeAdded()
		{
			string firstname = "firstname";
			string groupName = "groupName";

			Account crmAccount = CreateCrmAccount(firstname);
			Group crmGroup = Group.ReadOrCreate(_dynamicsCrmConnection, groupName);

			crmAccount.Insert();

			crmAccount.SynchronizeGroups(new List<Group>() { crmGroup });

			_synchronizeFromCrm.Execute();

			DatabaseExternalAccount databaseExternalAccount = DatabaseExternalAccount.Read(_sqlConnection, crmAccount.Id, _changeProvider.Id);
			DatabaseAccountChange databaseAccountchange = DatabaseAccountChange.Read(_sqlConnection, databaseExternalAccount.AccountId, DatabaseAccountChange.IdType.AccountId).
				Where(contactChange => contactChange.ChangeProviderId == _changeProvider.Id && contactChange.ExternalAccountId == databaseExternalAccount.ExternalAccountId).Single();

			List<DatabaseAccountChangeGroup> databaseAccountGroups = DatabaseAccountChangeGroup.ReadFromAccountChangeId(_sqlConnection, databaseAccountchange.Id);
			List<DatabaseGroup> databaseGroups = databaseAccountGroups.Select(contactGroup => DatabaseGroup.Read(_sqlConnection, contactGroup.GroupId)).ToList();

			crmAccount.Delete();
			crmGroup.Delete(_dynamicsCrmConnection);

			Assert.AreEqual(groupName, databaseGroups.Single().Name);
		}

		private Contact CreateCrmContact(string firstname1)
		{
			return new Contact(_dynamicsCrmConnection)
			{
				createdon = DateTime.Now,
				firstname = firstname1,
				lastname = "lastname1",
				modifiedon = DateTime.Now,
			};
		}

		private Account CreateCrmAccount(string name)
		{
			return new Account(_dynamicsCrmConnection)
			{
				name = name,
			};
		}
	}
}
