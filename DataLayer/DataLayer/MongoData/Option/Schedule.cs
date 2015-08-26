using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLayer.MongoData.Option
{
	public class Schedule
	{
		public bool Recurring { get; set; }
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime NextAllowedExecution { get; set; }
		public TimeSpan TimeBetweenAllowedExecutions { get; set; }
		public List<int> HoursOfDayToSkip { get; set; }
		public List<DayOfWeek> DaysOfWeekToSkip { get; set; }
		public List<int> DaysOfMonthToSkip { get; set; }

		public void MoveNext()
		{
			NextAllowedExecution += TimeBetweenAllowedExecutions;
			MoveToFreeTime();
		}

		private void MoveToFreeTime()
		{
			if (DayBlocksForExecution())
			{
				NextAllowedExecution = NextAllowedExecution.Date.AddDays(1);
				MoveToFreeTime();
				return;
			}
			if (HourBlocksForExecution())
			{
				int minutes = NextAllowedExecution.Minute;
				int seconds = NextAllowedExecution.Second;
				NextAllowedExecution = NextAllowedExecution.AddHours(1).AddMinutes(-minutes).AddSeconds(-seconds);
				MoveToFreeTime();
			}
		}

		private bool HourBlocksForExecution()
		{
			if (HoursOfDayToSkip.Contains(NextAllowedExecution.Hour))
			{
				return true;
			}
			return false;
		}

		private bool DayBlocksForExecution()
		{
			if (DaysOfWeekToSkip.Contains(NextAllowedExecution.DayOfWeek))
			{
				return true;
			}
			if (DaysOfMonthToSkip.Contains(NextAllowedExecution.Day))
			{
				return true;
			}
			return false;
		}
	}
}
