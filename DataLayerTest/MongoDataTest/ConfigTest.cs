using System;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class ConfigTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.DropDatabase();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_connection.DropDatabase();
		}

		[Test]
		public void GetConfigFailsForEmptyDatabase()
		{
			Action testAction = () => Config.GetConfig(_connection);

			Assert.Throws<AggregateException>(new TestDelegate(testAction));
		}

		[Test]
		public void GetConfigReturnsValues()
		{
			Config config = InsertData();

			Config configReturned = Config.GetConfig(_connection);

			Assert.AreEqual(config.EmailPassword, configReturned.EmailPassword);
		}

		[Test]
		public void ExistsOnEmptyReturnsFalse()
		{
			bool existOnEmpty = Config.Exists(_connection);

			Assert.False(existOnEmpty);
		}

		[Test]
		public void ExistsOnFilledReturnsTrue()
		{
			InsertData();

			bool existOnFilled = Config.Exists(_connection);

			Assert.True(existOnFilled);
		}

		[Test]
		public void Update()
		{
			InsertData();
			Config config = Config.GetConfig(_connection);

			string newEmail = Guid.NewGuid().ToString();

			config.Email = newEmail;
			config.Update(_connection);

			config = Config.GetConfig(_connection);
			Assert.AreEqual(newEmail, config.Email);
		}

		private Config InsertData()
		{
			Config config = new Config()
			{
				EmailPassword = "testPassword",
				Email = "testEmail",
				EmailSmtpPort = 0,
				EmailSmtpHost = "testHost",
			};

			config.Insert(_connection);

			return config;
		}
	}
}
