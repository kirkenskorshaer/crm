using System.Configuration;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace AdministrationTest
{
	[TestFixture]
	public class TestBase
	{
		protected MongoConnection Connection;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			Connection = MongoConnection.GetConnection(databaseName);
		}

		[SetUp]
		public void SetUp()
		{
			Connection.DropDatabase();

			Config config = new Config()
			{
				EmailPassword = "testPassword",
				Email = "testEmail",
				EmailSmtpPort = 0,
				EmailSmtpHost = "testHost",
			};

			config.Insert(Connection);
		}
	}
}
