using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class ServerTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();
		}

		[TearDown]
		public void TearDown()
		{
			_connection.CleanDatabase();
		}

		[Test]
		public void GetFirstTest()
		{
			_connection.CleanDatabase();
			Server serverInserted = InsertServer();

			Server serverReturned = Server.GetFirst(_connection);

			bool returnedIsInserted = serverInserted.Ip == serverReturned.Ip;

			Assert.True(returnedIsInserted);
		}

		[Test]
		public void GetServerRetreivesServer()
		{
			Server serverOrigin = InsertServer();

			Server serverRestored = Server.GetServer(_connection, serverOrigin.Ip);

			Assert.AreEqual(serverOrigin.Password, serverRestored.Password);
		}

		[Test]
		public void ExistsReturnFalseIfNoServerExists()
		{
			_connection.CleanDatabase();

			string ip = "127.0.0.1";

			bool exists = Server.Exists(_connection, ip);

			Assert.False(exists);
		}

		[Test]
		public void ExistsReturnFalseIfOtherServerExists()
		{
			_connection.CleanDatabase();

			InsertServer();
			string ip = "127.0.0.2";

			bool exists = Server.Exists(_connection, ip);

			Assert.False(exists);
		}

		[Test]
		public void ExistsReturnTrueIfServerExists()
		{
			Server server = InsertServer();

			bool exists = Server.Exists(_connection, server.Ip);

			Assert.True(exists);
		}

		private Server InsertServer()
		{
			string ip = "127.0.0.1";

			Server server;

			if (Server.Exists(_connection, ip) == false)
			{
				server = new Server()
				{
					Ip = ip,
					Password = "testPassword",
					Username = "testUsername",
				};

				server.Insert(_connection);
			}
			else
			{
				server = Server.GetServer(_connection, ip);
			}

			return server;
		}
	}
}
