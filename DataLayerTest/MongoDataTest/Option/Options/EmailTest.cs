using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options;
using NUnit.Framework;

namespace DataLayerTest.MongoDataTest.Option.Options
{
	[TestFixture]
	public class EmailTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.DropDatabase();
		}

		[Test]
		public void Read()
		{
			Email emailCreated = CreateEmail();
			List<Email> emails = Email.Read(_connection, emailCreated.Id);
			Email emailRetreived = emails.Single();

			AssertEmail(emailCreated, emailRetreived);
		}

		[Test]
		public void ReadAllowedTest()
		{
			Email emailCreatedAllowedIn2Days = CreateEmail();
			emailCreatedAllowedIn2Days.Schedule.NextAllowedExecution = emailCreatedAllowedIn2Days.Schedule.NextAllowedExecution.AddDays(2);
			emailCreatedAllowedIn2Days.Update(_connection);

			Email emailCreatedAllowed = CreateEmail();
			emailCreatedAllowed.Schedule.NextAllowedExecution = emailCreatedAllowed.Schedule.NextAllowedExecution.AddDays(-1);
			emailCreatedAllowed.Update(_connection);

			Email emailCreatedAllowedTomorrow = CreateEmail();
			emailCreatedAllowedTomorrow.Schedule.NextAllowedExecution = emailCreatedAllowedTomorrow.Schedule.NextAllowedExecution.AddDays(1);
			emailCreatedAllowedTomorrow.Update(_connection);

			List<Email> emails = OptionBase.ReadAllowed<Email>(_connection);
			Email emailRetreived = emails.Single();

			AssertEmail(emailCreatedAllowed, emailRetreived);
		}

		[Test]
		public void Update()
		{
			Email emailCreated = CreateEmail();

			emailCreated.MessageBody = "updated";
			emailCreated.Schedule.DaysOfMonthToSkip.Add(9);
			emailCreated.Update(_connection);

			List<Email> emails = Email.Read(_connection, emailCreated.Id);
			Email emailRetreived = emails.Single();

			AssertEmail(emailCreated, emailRetreived);
		}

		[Test]
		public void Delete()
		{
			Email emailCreated = CreateEmail();
			emailCreated.Delete(_connection);

			List<Email> emails = Email.Read(_connection, emailCreated.Id);

			Assert.False(emails.Any());
		}

		private Email CreateEmail()
		{
			Schedule schedule = new Schedule()
			{
				DaysOfMonthToSkip = new List<int> {1, 2, 3},
				DaysOfWeekToSkip = new List<DayOfWeek> {DayOfWeek.Monday, DayOfWeek.Thursday},
				HoursOfDayToSkip = new List<int>() {3, 4, 5},
				Recurring = true,
				NextAllowedExecution = DateTime.Now,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(1),
			};

			Email emailCreated = Email.Create(_connection, "test", schedule, "svend.l@kirkenskorshaer.dk", "test");
			return emailCreated;
		}

		private void AssertEmail(Email emailCreated, Email emailRetreived)
		{
			Assert.AreEqual(emailCreated.Schedule.NextAllowedExecution.Year, emailRetreived.Schedule.NextAllowedExecution.Year);
			Assert.AreEqual(emailCreated.Schedule.NextAllowedExecution.Month, emailRetreived.Schedule.NextAllowedExecution.Month);
			Assert.AreEqual(emailCreated.Schedule.NextAllowedExecution.Day, emailRetreived.Schedule.NextAllowedExecution.Day);
			Assert.AreEqual(emailCreated.Schedule.NextAllowedExecution.Hour, emailRetreived.Schedule.NextAllowedExecution.Hour);
			Assert.AreEqual(emailCreated.Schedule.NextAllowedExecution.Minute, emailRetreived.Schedule.NextAllowedExecution.Minute);
			Assert.AreEqual(emailCreated.Schedule.NextAllowedExecution.Second, emailRetreived.Schedule.NextAllowedExecution.Second);

			Assert.AreEqual(emailCreated.Schedule.DaysOfMonthToSkip, emailRetreived.Schedule.DaysOfMonthToSkip);
			Assert.AreEqual(emailCreated.Schedule.DaysOfWeekToSkip, emailRetreived.Schedule.DaysOfWeekToSkip);
			Assert.AreEqual(emailCreated.Schedule.HoursOfDayToSkip, emailRetreived.Schedule.HoursOfDayToSkip);
			Assert.AreEqual(emailCreated.Schedule.Recurring, emailRetreived.Schedule.Recurring);
			Assert.AreEqual(emailCreated.Schedule.TimeBetweenAllowedExecutions,
				emailRetreived.Schedule.TimeBetweenAllowedExecutions);

			Assert.AreEqual(emailCreated.MessageBody, emailRetreived.MessageBody);
			Assert.AreEqual(emailCreated.To, emailRetreived.To);
			Assert.AreEqual(emailCreated.Name, emailRetreived.Name);
			Assert.AreEqual(emailCreated.Id, emailRetreived.Id);
		}
	}
}
