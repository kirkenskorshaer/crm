using DataLayer;
using DataLayer.MongoData.Option;
using NUnit.Framework;
using System;

namespace DataLayerTest
{
	[TestFixture]
	public class TestBase
	{
		protected MongoConnection _mongoConnection;
		protected Random _random = new Random();

		[OneTimeSetUp]
		public void SetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
			_mongoConnection.CleanDatabase();
		}

		protected Schedule CreateSchedule()
		{
			Schedule schedule = new Schedule()
			{
				NextAllowedExecution = DateTime.Now,
			};

			return schedule;
		}
	}
}
