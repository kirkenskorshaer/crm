using NUnit.Framework;
using System.Data.SqlClient;
using SystemInterface.Dynamics.Crm;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseSynchronizeFromCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCrm;
using System.Linq;
using System.Collections.Generic;
using Administration.Option.Options.Logic;
using System;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SynchronizeFromCrmTest : TestBase
	{
		private SqlConnection _sqlConnection;
		private DataLayer.MongoData.UrlLogin _urlLogin;
		private DynamicsCrmConnection _dynamicsCrmConnection;
		private DatabaseChangeProvider _changeProvider;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_sqlConnection = DataLayer.SqlConnectionHolder.GetConnection(Connection, "sql");
			_urlLogin = DataLayer.MongoData.UrlLogin.GetUrlLogin(Connection, "test");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(_urlLogin.Url, _urlLogin.Username, _urlLogin.Password);

			_changeProvider = FindOrCreateChangeProvider(_sqlConnection, "testCrmProvider");
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
			DatabaseSynchronizeFromCrm databaseSynchronizeFromCrm = GetDatabaseSynchronizeFromCrm();
			SynchronizeFromCrm synchronizeFromCrm = new SynchronizeFromCrm(Connection, databaseSynchronizeFromCrm);
			synchronizeFromCrm.Execute();

			string firstname1 = "firstname1";
			string firstname2 = "firstname2";

			Contact crmContact = new Contact
			{
				CreatedOn = DateTime.Now,
				Firstname = firstname1,
				Lastname = "lastname1",
				ModifiedOn = DateTime.Now,
			};

			crmContact.Insert(_dynamicsCrmConnection);
			synchronizeFromCrm.Execute();

			crmContact.Firstname = firstname2;
			crmContact.Update(_dynamicsCrmConnection);
			synchronizeFromCrm.Execute();

			List<DataLayer.SqlData.Contact.ContactChange> contactChanges = DataLayer.SqlData.Contact.ContactChange.Read(_sqlConnection, crmContact.ContactId, DataLayer.SqlData.Contact.ContactChange.IdType.ExternalContactId);

			crmContact.Delete(_dynamicsCrmConnection);

			Assert.AreEqual(2, contactChanges.Count);
			Assert.IsTrue(contactChanges.Any(contactChange => contactChange.Firstname == firstname1));
			Assert.IsTrue(contactChanges.Any(contactChange => contactChange.Firstname == firstname2));
		}
	}
}
