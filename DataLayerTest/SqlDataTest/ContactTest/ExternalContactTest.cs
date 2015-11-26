using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Contact;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.ContactTest
{
	[TestFixture]
	public class ExternalContactTest : TestSqlBase
	{
		private MongoConnection _mongoConnection;
		private SqlConnection _sqlConnection;

		private List<ChangeProvider> _changeProviders = new List<ChangeProvider>();

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");

			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
		}

		[SetUp]
		public void SetUp()
		{
			if (Utilities.GetExistingColumns(_sqlConnection, typeof(ExternalContact).Name).Any())
			{
				if (Utilities.GetExistingColumns(_sqlConnection, typeof(ContactChange).Name).Any())
				{
					Utilities.DropTable(_sqlConnection, typeof(ContactChange).Name);
				}
				Utilities.DropTable(_sqlConnection, typeof(ExternalContact).Name);
			}
			ExternalContact.MaintainTable(_sqlConnection);
			ContactChange.MaintainTable(_sqlConnection);
		}

		[TearDown]
		public void TearDown()
		{
			_changeProviders.ForEach(changeProvider => changeProvider.Delete(_sqlConnection));

			_changeProviders.Clear();
		}

		private ChangeProvider InsertChangeProvider()
		{
			ChangeProvider changeProvider = new ChangeProvider();
			changeProvider.Name = $"name_{Guid.NewGuid()}";
			changeProvider.Insert(_sqlConnection);

			_changeProviders.Add(changeProvider);

			return changeProvider;
		}

		[Test]
		public void ReadReadsInserted()
		{
			ChangeProvider changeProvider = InsertChangeProvider();
			Guid externalContactId = Guid.NewGuid();

			ExternalContact externalContactCreated = new ExternalContact(_sqlConnection, externalContactId, changeProvider.Id);
			externalContactCreated.Insert();

			ExternalContact externalContactRead = ExternalContact.Read(_sqlConnection, externalContactId, changeProvider.Id);

			Assert.AreEqual(externalContactCreated.ChangeProviderId, externalContactRead.ChangeProviderId);
			Assert.AreEqual(externalContactCreated.ExternalContactId, externalContactRead.ExternalContactId);
		}

		[Test]
		public void ReadReadsAListFromChangeProviderId()
		{
			ChangeProvider changeProvider = InsertChangeProvider();

			Guid externalContact1Id = Guid.NewGuid();
			ExternalContact externalContact1Created = new ExternalContact(_sqlConnection, externalContact1Id, changeProvider.Id);
			externalContact1Created.Insert();

			Guid externalContact2Id = Guid.NewGuid();
			ExternalContact externalContact2Created = new ExternalContact(_sqlConnection, externalContact2Id, changeProvider.Id);
			externalContact2Created.Insert();

			List<ExternalContact> externalContactReadList = ExternalContact.Read(_sqlConnection, changeProvider.Id);

			Assert.AreEqual(2, externalContactReadList.Count);
			Assert.IsTrue(externalContactReadList.Any(externalContact => externalContact.ExternalContactId == externalContact1Id));
			Assert.IsTrue(externalContactReadList.Any(externalContact => externalContact.ExternalContactId == externalContact2Id));
		}

		[Test]
		public void ReadFromChangeProviderAndContact()
		{
			ChangeProvider changeProvider = InsertChangeProvider();

			Guid externalContactId = Guid.NewGuid();
			ExternalContact externalContactCreated = new ExternalContact(_sqlConnection, externalContactId, changeProvider.Id);
			externalContactCreated.Insert();

			Contact contact = InsertContact(_sqlConnection);

			InsertContactChange(_sqlConnection, contact.Id, externalContactId, changeProvider.Id, DateTime.Now);
			InsertContactChange(_sqlConnection, contact.Id, externalContactId, changeProvider.Id, DateTime.Now);

			List<ExternalContact> externalContactRead = ExternalContact.ReadFromChangeProviderAndContact(_sqlConnection, changeProvider.Id, contact.Id);

			Assert.AreEqual(externalContactCreated.ChangeProviderId, externalContactRead.Single().ChangeProviderId);
			Assert.AreEqual(externalContactCreated.ExternalContactId, externalContactRead.Single().ExternalContactId);
		}
	}
}
