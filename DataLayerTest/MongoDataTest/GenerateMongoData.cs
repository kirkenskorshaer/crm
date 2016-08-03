using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options.Logic;
using NUnit.Framework;
using System;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class GenerateMongoData : TestBase
	{
		[Test]
		public void CreateUpdateMailrelayFromContact()
		{
			UpdateMailrelayFromContact.Create(_mongoConnection, "test", "test", GreateSchedule());
		}

		private Schedule GreateSchedule()
		{
			return new Schedule()
			{
				NextAllowedExecution = new DateTime(2000, 1, 1),
				Recurring = true,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(10),
			};
		}
	}
}
