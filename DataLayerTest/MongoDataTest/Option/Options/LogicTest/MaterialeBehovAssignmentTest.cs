using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DataLayerTest.MongoDataTest.Option.Options.LogicTest
{
	[TestFixture]
	public class MaterialeBehovAssignmentTest : TestBase
	{
		[Test]
		public void CreateMaterialeBehovAssignment()
		{
			Schedule schedule = new Schedule()
			{
				DaysOfMonthToSkip = new List<int>(),
				DaysOfWeekToSkip = new List<DayOfWeek>(),
				HoursOfDayToSkip = new List<int>(),
				NextAllowedExecution = DateTime.Now,
				Recurring = true,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(1),
			};

			MaterialeBehovAssignment assignment = MaterialeBehovAssignment.Create(_mongoConnection, "test", schedule, "test");
		}
	}
}
