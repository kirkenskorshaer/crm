using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Data.SqlClient;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest
{
	[TestFixture]
	public class TestBase
	{
		protected MongoConnection _mongoConnection;
		protected SqlConnection _sqlConnection;
		protected DynamicsCrmConnection _dynamicsCrmConnection;
		protected UrlLogin _urlLogin;
		protected Config _config;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
			_urlLogin = UrlLogin.GetUrlLogin(_mongoConnection, "test");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(_urlLogin.Url, _urlLogin.Username, _urlLogin.Password);
			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
			_config = Config.GetConfig(_mongoConnection);
		}

		protected Contact CreateTestContact()
		{
			return CreateTestContact(DateTime.Now);
		}

		protected Contact CreateTestContact(DateTime testDate)
		{
			string dateString = testDate.ToString("yyyy_MM_dd_HH_mm_ss");
			Contact contactCreated = new Contact(_dynamicsCrmConnection)
			{
				firstname = $"firstname_{dateString}",
				lastname = $"lastname_{dateString}",
			};

			return contactCreated;
		}

		protected Campaign CreateTestCampaign(DateTime testDate)
		{
			string dateString = testDate.ToString("yyyy_MM_dd_HH_mm_ss");
			Campaign campaignCreated = new Campaign(_dynamicsCrmConnection)
			{
				name = $"name_{dateString}",
			};

			return campaignCreated;
		}
	}
}
