using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Contact;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.SqlDataTest.ContactTest
{
	[TestFixture]
	public class ExternalContactTest
	{
		private MongoConnection _mongoConnection;
		private SqlConnection _sqlConnection;

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
		}

		private ChangeProvider InsertChangeProvider()
		{
			ChangeProvider changeProvider = new ChangeProvider();
			changeProvider.Name = $"name_{Guid.NewGuid()}";
			changeProvider.Insert(_sqlConnection);
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

			changeProvider.Delete(_sqlConnection);

			Assert.AreEqual(externalContactCreated.ChangeProviderId, externalContactRead.ChangeProviderId);
			Assert.AreEqual(externalContactCreated.ExternalContactId, externalContactRead.ExternalContactId);
		}
	}
}
