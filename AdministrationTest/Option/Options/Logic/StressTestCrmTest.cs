using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseStressTestCrm = DataLayer.MongoData.Option.Options.Logic.StressTestCrm;
using DatabaseStringIntStatistics = DataLayer.MongoData.Statistics.StringStatistics.StringIntStatistics;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class StressTestCrmTest : TestBase
	{
		[Test]
		public void StressTestCrmCreatesTheCroeecteNumberOfStatisticsEntries()
		{
			DatabaseStressTestCrm databaseStressTestCrm = GetDatabaseStressTestCrm();

			StressTestCrm stressTestCrm = new StressTestCrm(Connection, databaseStressTestCrm);

			stressTestCrm.Execute();
			stressTestCrm.Execute();

			List<DatabaseStringIntStatistics> statisticsList = DatabaseStringIntStatistics.ReadByName(Connection, "StressTestCrm");

			Assert.AreEqual(2, statisticsList.Count);
		}

		private DatabaseStressTestCrm GetDatabaseStressTestCrm()
		{
			DatabaseStressTestCrm databaseStressTestCrm = DatabaseStressTestCrm.Create(Connection, "StressTestCrm", CreateScheduleAlwaysOnDoOnce(), "test", 10);

			return databaseStressTestCrm;
		}
	}
}
