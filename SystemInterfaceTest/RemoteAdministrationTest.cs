using System;
using NUnit.Framework;
using SystemInterface;
using DataLayer;
using DataLayer.MongoData;

namespace SystemInterfaceTest
{
	[TestFixture]
	public class RemoteAdministrationTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void Setup()
		{
			_connection = MongoConnection.GetConnection("test");
		}

		[Test]
		public void ConnectTest()
		{
			RemoteAdministration remoteAdministration = new RemoteAdministration();

			Server server = Server.GetFirst(_connection);

			string path = "C:/test/testfile.txt";
			string testText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			remoteAdministration.CreateFile(server.Ip, server.Username, server.Password, path, testText);
		}

		[Test]
		public void ServiceExistsRetunsTrueOnExistingService()
		{
			RemoteAdministration remoteAdministration = new RemoteAdministration();
			Server server = Server.GetFirst(_connection);

			bool exists = remoteAdministration.ServiceExists(server.Ip, server.Username, server.Password, "WinRM");

			Assert.True(exists);
		}

		[Test]
		public void ServiceExistsRetunsFalseOnNonExistingService()
		{
			RemoteAdministration remoteAdministration = new RemoteAdministration();
			Server server = Server.GetFirst(_connection);

			bool exists = remoteAdministration.ServiceExists(server.Ip, server.Username, server.Password, Guid.NewGuid().ToString());

			Assert.False(exists);
		}
	}
}
