using DataLayer.MongoData.Statistics.StringStatistics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DataLayerTest.MongoDataTest.StatisticsTest.StringStatisticsTest
{
	[TestFixture]
	public class StringIntStatisticsTest : AbstratStatisticsTest
	{
		[Test]
		public void DataWrittenCanBeRead()
		{
			StringIntStatistics stringIntStatisticsCreated = StringIntStatistics.Create(Connection, "test", "_100", 10);

			List<StringIntStatistics> stringIntStatisticsRead = StringIntStatistics.Read(Connection, stringIntStatisticsCreated.Id);

			Assert.AreEqual(stringIntStatisticsCreated.XValue, stringIntStatisticsRead.Single().XValue);
			Assert.AreEqual(stringIntStatisticsCreated.YValue, stringIntStatisticsRead.Single().YValue);
		}
	}
}
