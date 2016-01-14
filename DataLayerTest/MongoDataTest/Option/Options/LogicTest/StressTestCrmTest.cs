using DataLayer;
using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.MongoDataTest.Option.Options.LogicTest
{
	[TestFixture]
	public class StressTestCrmTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();
		}

		[Test]
		public void InsertAndDelete()
		{
			StressTestCrm stressTestCrm = CreateStressTestCrm();

			stressTestCrm.Delete(_connection);
        }

		private StressTestCrm CreateStressTestCrm()
		{
			Schedule schedule = MongoTestUtilities.CreateSchedule();

			StressTestCrm stressTestCrm = StressTestCrm.Create(_connection, "stressTest", schedule, "test", 10, 1);

			return stressTestCrm;
		}
	}
}
