using System;
using System.Collections.Generic;
using System.Configuration;
using DataLayer;
using DataLayer.MongoData;
using DataLayer.MongoData.Option;
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
			Connection.CleanDatabase();

			Config config = new Config()
			{
				EmailPassword = "testPassword",
				Email = "testEmail",
				EmailSmtpPort = 0,
				EmailSmtpHost = "testHost",
			};

			config.Insert(Connection);
		}

		protected Schedule CreateSchedule()
		{
			Schedule schedule = new Schedule()
			{
				DaysOfMonthToSkip = new List<int> { 1, 2, 3 },
				DaysOfWeekToSkip = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Thursday },
				HoursOfDayToSkip = new List<int>() { 3, 4, 5 },
				Recurring = true,
				NextAllowedExecution = DateTime.Now,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(1),
			};
			return schedule;
		}
	}
}
