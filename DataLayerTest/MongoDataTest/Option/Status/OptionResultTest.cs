using DataLayer.MongoData.Option.Status;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Utilities;

namespace DataLayerTest.MongoDataTest.Option.Status
{
	[TestFixture]
	public class OptionResultTest : TestBase
	{
		[Test]
		[Ignore("")]
		public void GetOptionStatusHandlesLargeAmmountsOfData()
		{
			for (int index = 0; index < 1000 * 1000; index++)
			{
				DateTime endTime = GetEndTime();
				DateTime beginTime = GetBeginTime(endTime);
				string name = GetName();
				bool success = GetSuccess();

				OptionResult.Create(_mongoConnection, beginTime, endTime, name, success, 0);
			}

			DateTime start = DateTime.Now;

			OptionResult.GetOptionStatus(_mongoConnection);

			DateTime slut = DateTime.Now;

			Assert.Less(slut - start, TimeSpan.FromMinutes(1));
		}

		[Test]
		public void GetOptionStatusGivesCorrectData()
		{
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddMinutes(-1), "name1", true, 0);
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddMinutes(-12), "name1", true, 0);
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddMinutes(-65), "name1", true, 0);
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddHours(-20), "name1", false, 0);
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddHours(-30), "name1", false, 0);
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddMinutes(-5), "name2", true, 0);
			OptionResult.Create(_mongoConnection, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1).AddMinutes(-5), "name2", false, 0);

			Clock.NowFunc = () => new DateTime(2000, 1, 1);

			Dictionary<string, OptionStatusLine> optionStatus = OptionResult.GetOptionStatus(_mongoConnection);

			Assert.AreEqual(5, optionStatus["name1"].ExecutionTotal);
			Assert.AreEqual(0, optionStatus["name1"].Fail10Minute);
			Assert.AreEqual(0, optionStatus["name1"].Fail1Hour);
			Assert.AreEqual(1, optionStatus["name1"].Fail24Hour);
			Assert.AreEqual(2, optionStatus["name1"].FailTotal);
			Assert.AreEqual(1, optionStatus["name1"].Success10Minute);
			Assert.AreEqual(2, optionStatus["name1"].Success1Hour);
			Assert.AreEqual(3, optionStatus["name1"].Success24Hour);
			Assert.AreEqual(3, optionStatus["name1"].SuccessTotal);
		}

		[Test]
		public void ClearOldResultsDeletesOldResults()
		{
			DateTime originTime = new DateTime(2000, 1, 1);

			OptionResult.Create(_mongoConnection, originTime, originTime.AddDays(-10), "before 1", true, 0);
			OptionResult.Create(_mongoConnection, originTime, originTime.AddDays(-5), "before 2", true, 0);

			OptionResult.Create(_mongoConnection, originTime, originTime.AddDays(5), "after 1", true, 0);

			long deleted = OptionResult.ClearOldResults(_mongoConnection, originTime);

			Assert.AreEqual(2, deleted);
		}

		private bool GetSuccess()
		{
			return _random.Next(0, 2) == 1;
		}

		private string GetName()
		{
			return $"name_{_random.Next(0, 10)}";
		}

		private DateTime GetEndTime()
		{
			return DateTime.Now - TimeSpan.FromMinutes(_random.Next(1, 60 * 24 * 366));
		}

		private DateTime GetBeginTime(DateTime endTime)
		{
			return endTime - TimeSpan.FromMinutes(_random.Next(1, 60 * 24 * 366));
		}
	}
}
