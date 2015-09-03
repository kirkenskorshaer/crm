using System;
using System.Configuration;
using System.Diagnostics;
using Administration;
using DataLayer;
using DataLayer.MongoData;
using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options;
using NUnit.Framework;

namespace AdministrationTest
{
	[TestFixture]
	public class HeartTest
	{
		private MongoConnection _connection;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			_connection = MongoConnection.GetConnection(databaseName);
		}

		[SetUp]
		public void SetUp()
		{
			_connection.DropDatabase();

			Config config = new Config()
			{
				EmailPassword = "testPassword",
				Email = "testEmail",
				EmailSmtpPort = 0,
				EmailSmtpHost = "testHost",
			};

			config.Insert(_connection);
		}

		[Test]
		public void HeartBeatPrefersEmail()
		{
			Email.Create(_connection, "testName", new Schedule(), "testTo", "testMessageBody");

			Heart heart = new Heart();

			try
			{
				heart.HeartBeat();
			}
			catch (FormatException exception)
			{
				Assert.AreEqual("The specified string is not in the form required for an e-mail address.", exception.Message);
				return;
			}

			Assert.Fail();
		}

		[Test]
		public void HeartBeatSleepsWhenThereIsNothingToDo()
		{
			Heart heart = new Heart();

			Stopwatch stopwatch = Stopwatch.StartNew();
			heart.HeartBeat();
			stopwatch.Stop();

			Assert.Greater(stopwatch.ElapsedMilliseconds, 100);
		}

		[Test]
		[Ignore]
		public void RunTest()
		{
			Heart heart = new Heart();
			heart.Run();
		}
	}
}
