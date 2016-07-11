using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Data.SqlClient;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay;

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
		protected MailrelayConnection _mailrelayConnection;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
			_urlLogin = UrlLogin.GetUrlLogin(_mongoConnection, "test");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(_urlLogin.Url, _urlLogin.Username, _urlLogin.Password);
			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
			_config = Config.GetConfig(_mongoConnection);
			_mailrelayConnection = new MailrelayConnection(_config.MailrelayUrl, _config.MailrelayApiKey);
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
	}
}
