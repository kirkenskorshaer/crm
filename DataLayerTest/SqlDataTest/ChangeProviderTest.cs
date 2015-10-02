using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Contact;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest
{
	[TestFixture]
	public class ChangeProviderTest
	{
		private MongoConnection _mongoConnection;
		private SqlConnection _sqlConnection;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");

			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "testMssql");
		}

		[SetUp]
		public void SetUp()
		{
			if (Utilities.GetExistingColumns(_sqlConnection, typeof(ChangeProvider).Name).Any())
			{
				if (Utilities.GetExistingColumns(_sqlConnection, typeof(ExternalContact).Name).Any())
				{
					Utilities.DropTable(_sqlConnection, typeof(ExternalContact).Name);
				}
				if (Utilities.GetExistingColumns(_sqlConnection, typeof(ContactChange).Name).Any())
				{
					Utilities.DropTable(_sqlConnection, typeof(ContactChange).Name);
				}
				Utilities.DropTable(_sqlConnection, typeof(ChangeProvider).Name);
			}
			ChangeProvider.MaintainTable(_sqlConnection);
		}

		private ChangeProvider InsertChangeProvider()
		{
			ChangeProvider provider = new ChangeProvider()
			{
				Name = $"name_{Guid.NewGuid()}",
			};

			provider.Insert(_sqlConnection);

			return provider;
		}

		[Test]
		public void InsertTest()
		{
			List<ChangeProvider> changeProvidersBeforeInsert = ChangeProvider.ReadAll(_sqlConnection);

			InsertChangeProvider();

			List<ChangeProvider> changeProvidersAfterInsert = ChangeProvider.ReadAll(_sqlConnection);
			Assert.AreEqual(changeProvidersBeforeInsert.Count + 1, changeProvidersAfterInsert.Count);
		}

		[Test]
		public void ReadReadsCorrectChangeProvider()
		{
			ChangeProvider changeProvider1 = InsertChangeProvider();
			ChangeProvider changeProvider2 = InsertChangeProvider();
			ChangeProvider changeProvider3 = InsertChangeProvider();
			ChangeProvider changeProvider4 = InsertChangeProvider();
			ChangeProvider changeProvider5 = InsertChangeProvider();

			ChangeProvider changeProviderRead = ChangeProvider.Read(_sqlConnection, changeProvider3.Id);

			changeProvider1.Delete(_sqlConnection);
			changeProvider2.Delete(_sqlConnection);
			changeProvider3.Delete(_sqlConnection);
			changeProvider4.Delete(_sqlConnection);
			changeProvider5.Delete(_sqlConnection);

			Assert.AreEqual(changeProvider3.Name, changeProviderRead.Name);
		}
	}
}
