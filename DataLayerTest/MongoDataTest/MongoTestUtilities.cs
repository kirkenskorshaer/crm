using DataLayer.MongoData.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.MongoDataTest
{
	public static class MongoTestUtilities
	{
		public static Schedule CreateSchedule()
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

		public static Schedule CreateOneTimeSimpleSchedule()
		{
			Schedule schedule = new Schedule()
			{
				Recurring = false,
				NextAllowedExecution = DateTime.Now,
			};
			return schedule;
		}
	}
}
