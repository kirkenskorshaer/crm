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
		public void CreateFileTest()
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

		[Test]
		public void CopyFileTest()
		{
			RemoteAdministration remoteAdministration = new RemoteAdministration();
			Server server = Server.GetFirst(_connection);

			remoteAdministration.CopyFile(server.Ip, server.Username, server.Password, "C:/test/testfile.txt", "C:/test/testfile_copy.txt");
			remoteAdministration.CopyFile(server.Ip, server.Username, server.Password, "C:/Users/Svend/Documents/GitHub/crm/ServiceRunner/ServiceRunner/bin/Release/ServiceRunner.exe", "C:/test/testfolder/ServiceRunner.exe");
		}
	}
}
