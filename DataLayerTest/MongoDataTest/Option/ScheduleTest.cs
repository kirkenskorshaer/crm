using System;
using System.Collections.Generic;
using DataLayer.MongoData.Option;
using NUnit.Framework;
using DataLayer.MongoData.Option.Options.Logic;
using DataLayer.MongoData;
using System.Linq;

namespace DataLayerTest.MongoDataTest.Option
{
	[TestFixture]
	public class ScheduleTest : TestBase
	{
		[TestCase(25, 0, 67, 25, 1, 7)]
		[TestCase(22, 0, 67, 24, 0, 0)]
		[TestCase(25, 9, 67, 25, 18, 0)]
		[TestCase(10, 19, 67, 17, 0, 0)]
		public void MoveNextTest(int dayOrigin, int hourOrigin, int minutesBetweenExecution, int dayExpected, int hourExpected, int minuteExpected)
		{
			Schedule schedule = new Schedule()
			{
				DaysOfMonthToSkip = new List<int> { 1, 2, 3, 4, 5, 10, 11, 12, 13, 14, 15 },
				DaysOfWeekToSkip = new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday },
				HoursOfDayToSkip = new List<int>() { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 },
				Recurring = true,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(minutesBetweenExecution),
				NextAllowedExecution = new DateTime(2015, 8, dayOrigin, hourOrigin, 0, 0),
			};

			Utilities.Clock.NowFunc = () => new DateTime(2015, 1, 1);

			schedule.MoveNext();

			Assert.AreEqual(new DateTime(2015, 8, dayExpected, hourExpected, minuteExpected, 0).ToString("s"), schedule.NextAllowedExecution.ToString("s"));
		}

		[Test]
		public void AssignWorkerAssignsWorkerToAll()
		{
			SumIndbetaling option1 = SumIndbetaling.Create(_mongoConnection, "test", "test", CreateSchedule());
			SumIndbetaling option2 = SumIndbetaling.Create(_mongoConnection, "test", "test", CreateSchedule());
			SumIndbetaling option3 = SumIndbetaling.Create(_mongoConnection, "test", "test", CreateSchedule());

			Worker worker = new Worker();

			worker.Create(_mongoConnection);

			OptionBase.AssignWorkerToAllUnassigned<SumIndbetaling>(worker, _mongoConnection);

			List<SumIndbetaling> optionsRead = OptionBase.ReadAllowed<SumIndbetaling>(_mongoConnection, null);

			Assert.IsTrue(optionsRead.All(option => option.Schedule.WorkerId == worker._id));
		}
	}
}
