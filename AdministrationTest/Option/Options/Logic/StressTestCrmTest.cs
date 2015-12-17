using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseStressTestCrm = DataLayer.MongoData.Option.Options.Logic.StressTestCrm;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class StressTestCrmTest : TestBase
	{
		[Test]
		public void TestCrm()
		{
			DatabaseStressTestCrm databaseStressTestCrm = GetDatabaseStressTestCrm();

			StressTestCrm stressTestCrm = new StressTestCrm(Connection, databaseStressTestCrm);

			stressTestCrm.Execute();
		}

		private DatabaseStressTestCrm GetDatabaseStressTestCrm()
		{
			return new DatabaseStressTestCrm()
			{
				contactsToCreate = 100,
				Name = "StressTestCrm",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};
		}
	}
}
