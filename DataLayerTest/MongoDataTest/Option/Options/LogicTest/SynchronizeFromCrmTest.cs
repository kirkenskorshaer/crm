using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options.Logic;
using NUnit.Framework;
using System;

namespace DataLayerTest.MongoDataTest.Option.Options.LogicTest
{
	[TestFixture]
	public class SynchronizeFromCrmTest : TestBase
	{
		[Test]
		public void InsertAndDelete()
		{
			Schedule schedule = new Schedule()
			{
				NextAllowedExecution = DateTime.Now,
				Recurring = true,
				TimeBetweenAllowedExecutions = TimeSpan.FromMinutes(1),
			};

			SynchronizeFromCrm synchronizeFromCrm = SynchronizeFromCrm.Create(_mongoConnection, "test", schedule, "test", Guid.Parse("8C6A4E39-8350-4C6B-9085-6B6BE091BE6D"));

			synchronizeFromCrm.Delete(_mongoConnection);
		}
	}
}
