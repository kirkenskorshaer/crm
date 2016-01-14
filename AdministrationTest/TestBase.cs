using System;
using System.Collections.Generic;
using System.Configuration;
using DataLayer;
using DataLayer.MongoData;
using DataLayer.MongoData.Option;
using NUnit.Framework;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using System.Linq;
using System.Data.SqlClient;
using SystemInterface.Dynamics.Crm;

namespace AdministrationTest
{
	[TestFixture]
	public class TestBase
	{
		protected MongoConnection Connection;
		protected SqlConnection _sqlConnection;
		protected DynamicsCrmConnection DynamicsCrmConnection;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			Connection = MongoConnection.GetConnection(databaseName);
		}

		private List<DatabaseChangeProvider> _changeProviders = new List<DatabaseChangeProvider>();

		[SetUp]
		public void SetUp()
		{
			Connection.CleanDatabase();
			_sqlConnection = DataLayer.SqlConnectionHolder.GetConnection(Connection, "sql");

			if (Config.Exists(Connection) == false)
			{
				Config config = new Config()
				{
					EmailPassword = "testPassword",
					Email = "testEmail",
					EmailSmtpPort = 0,
					EmailSmtpHost = "testHost",
				};

				config.Insert(Connection);
			}

			UrlLogin urlLogin = UrlLogin.GetUrlLogin(Connection, "test");
			DynamicsCrmConnection = DynamicsCrmConnection.GetConnection(urlLogin.Url, urlLogin.Username, urlLogin.Password);
		}

		[TearDown]
		public void TearDown()
		{
			_changeProviders.ForEach(changeProvider => changeProvider.Delete(_sqlConnection));
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

		protected Schedule CreateScheduleAlwaysOnDoOnce()
		{
			Schedule schedule = new Schedule()
			{
				DaysOfMonthToSkip = new List<int> { },
				DaysOfWeekToSkip = new List<DayOfWeek> { },
				HoursOfDayToSkip = new List<int>() { },
				Recurring = false,
				NextAllowedExecution = DateTime.Now,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(1),
			};
			return schedule;
		}

		protected DatabaseChangeProvider FindOrCreateChangeProvider(SqlConnection sqlConnection, string providerName)
		{
			List<DatabaseChangeProvider> changeProviders = DatabaseChangeProvider.ReadAll(sqlConnection);

			Func<DatabaseChangeProvider, bool> findChangeProvider = lChangeProvider => lChangeProvider.Name == providerName;

			DatabaseChangeProvider changeProvider;

			if (changeProviders.Any(findChangeProvider))
			{
				changeProvider = changeProviders.Single(findChangeProvider);
			}
			else
			{
				changeProvider = new DatabaseChangeProvider()
				{
					Name = providerName,
				};

				changeProvider.Insert(sqlConnection);
			}

			_changeProviders.Add(changeProvider);

			return changeProvider;
		}
	}
}
