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
			_connection.CleanDatabase();
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_connection.CleanDatabase();
		}

		[Test]
		[Ignore("")]
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
		[Ignore("")]
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

		[Test]
		public void ConfigWithResourcePathReturnsCorrectPath()
		{
			InsertData();
			Config config = Config.GetConfig(_connection);

			config.ResourcePath = "c:/home";

			string localPath = "test/file.txt";

			string pathRead = config.GetResourcePath(localPath);

			Assert.AreEqual(pathRead, config.ResourcePath + "/" + localPath);
		}

		[Test]
		public void ConfigWithoutResourcePathReturnsAPath()
		{
			InsertData();
			Config config = Config.GetConfig(_connection);

			config.ResourcePath = string.Empty;

			string localPath = "test/file.txt";

			string pathRead = config.GetResourcePath(localPath);

			Assert.Greater(pathRead.Length, localPath.Length + 5);
		}

		private Config InsertData()
		{
			Config config;
			bool configAlreadyExist = Config.Exists(_connection);

			if (configAlreadyExist)
			{
				config = Config.GetConfig(_connection);
			}
			else
			{
				config = new Config();
			}

			config.EmailPassword = "testPassword";
			config.Email = "testEmail";
			config.EmailSmtpPort = 0;
			config.EmailSmtpHost = "testHost";

			if (configAlreadyExist)
			{
				config.Update(_connection);
			}
			else
			{
				config.Insert(_connection);
			}

			return config;
		}
	}
}
