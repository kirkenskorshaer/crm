using NUnit.Framework;
using SystemInterface.Dynamics.Crm;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseSynchronizeFromCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCrm;
using DatabaseAccountChangeContact = DataLayer.SqlData.Account.AccountChangeContact;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
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

			List<DataLayer.SqlData.Contact.ContactChange> contactChanges = DataLayer.SqlData.Contact.ContactChange.Read(_sqlConnection, crmContact.Id, DataLayer.SqlData.Contact.ContactChange.IdType.ExternalContactId);

			crmContact.Delete();

			Assert.AreEqual(2, contactChanges.Count);
			Assert.IsTrue(contactChanges.Any(contactChange => contactChange.Firstname == firstname1));
			Assert.IsTrue(contactChanges.Any(contactChange => contactChange.Firstname == firstname2));
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
