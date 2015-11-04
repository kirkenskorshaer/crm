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
			_connection.CleanDatabase();
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

		[Test]
		public void ExecuteUpdatesRecurring()
		{
			Email emailCreated = CreateEmail();

			emailCreated.Execute(_connection);

			List<Email> emails = Email.Read(_connection, emailCreated.Id);
			Schedule emailCreatedSchedule = MongoTestUtilities.CreateSchedule();
			emailCreatedSchedule.MoveNext();

			AssertSchedule(emailCreatedSchedule, emails.Single().Schedule);
		}

		[Test]
		public void ExecuteDeletesNonRecurring()
		{
			Email emailCreated = CreateEmail();
			emailCreated.Schedule.Recurring = false;
			emailCreated.Execute(_connection);

			List<Email> emails = Email.Read(_connection, emailCreated.Id);

			Assert.False(emails.Any());
		}

		private Email CreateEmail()
		{
			Schedule schedule = MongoTestUtilities.CreateSchedule();

			Email emailCreated = Email.Create(_connection, "test", schedule, "svend.l@kirkenskorshaer.dk", "test");
			return emailCreated;
		}

		private void AssertEmail(Email emailCreated, Email emailRetreived)
		{
			AssertSchedule(emailCreated.Schedule, emailRetreived.Schedule);

			Assert.AreEqual(emailCreated.MessageBody, emailRetreived.MessageBody);
			Assert.AreEqual(emailCreated.To, emailRetreived.To);
			Assert.AreEqual(emailCreated.Name, emailRetreived.Name);
			Assert.AreEqual(emailCreated.Id, emailRetreived.Id);
		}

		private void AssertSchedule(Schedule scheduleExpected, Schedule scheduleActual)
		{
			Assert.AreEqual(scheduleExpected.NextAllowedExecution.Year, scheduleActual.NextAllowedExecution.Year);
			Assert.AreEqual(scheduleExpected.NextAllowedExecution.Month, scheduleActual.NextAllowedExecution.Month);
			Assert.AreEqual(scheduleExpected.NextAllowedExecution.Day, scheduleActual.NextAllowedExecution.Day);
			Assert.AreEqual(scheduleExpected.NextAllowedExecution.Hour, scheduleActual.NextAllowedExecution.Hour);
			Assert.AreEqual(scheduleExpected.NextAllowedExecution.Minute, scheduleActual.NextAllowedExecution.Minute);
			//Assert.AreEqual(scheduleExpected.NextAllowedExecution.Second, scheduleActual.NextAllowedExecution.Second);

			Assert.AreEqual(scheduleExpected.DaysOfMonthToSkip, scheduleActual.DaysOfMonthToSkip);
			Assert.AreEqual(scheduleExpected.DaysOfWeekToSkip, scheduleActual.DaysOfWeekToSkip);
			Assert.AreEqual(scheduleExpected.HoursOfDayToSkip, scheduleActual.HoursOfDayToSkip);
			Assert.AreEqual(scheduleExpected.Recurring, scheduleActual.Recurring);
			Assert.AreEqual(scheduleExpected.TimeBetweenAllowedExecutions, scheduleActual.TimeBetweenAllowedExecutions);
		}
	}
}
