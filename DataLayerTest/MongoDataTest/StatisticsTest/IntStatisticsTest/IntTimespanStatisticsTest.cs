using DataLayer.MongoData.Statistics.IntStatistics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.MongoDataTest.StatisticsTest.IntStatisticsTest
{
	[TestFixture]
	public class IntTimespanStatisticsTest : AbstratStatisticsTest
	{
		[Test]
		public void test()
		{
			IntTimespanStatistics intTimespanStatistics1 = IntTimespanStatistics.Create(Connection, "test", 1, 10);
			IntTimespanStatistics intTimespanStatistics2 = IntTimespanStatistics.Create(Connection, "test", 3, 30);
			IntTimespanStatistics intTimespanStatistics3 = IntTimespanStatistics.Create(Connection, "test", 8, 80);
			IntTimespanStatistics intTimespanStatistics4 = IntTimespanStatistics.Create(Connection, "test", 2, 20);
			IntTimespanStatistics intTimespanStatistics5 = IntTimespanStatistics.Create(Connection, "test", 5, 50);

		}
	}
}
