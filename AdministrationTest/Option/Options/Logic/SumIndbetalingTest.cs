using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseSumIndbetaling = DataLayer.MongoData.Option.Options.Logic.SumIndbetaling;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SumIndbetalingTest : TestBase
	{
		[Test]
		public void test()
		{
			DatabaseSumIndbetaling databaseSumIndbetaling = CreateDatabaseSumIndbetaling();

			SumIndbetaling sumIndbetaling = new SumIndbetaling(Connection, databaseSumIndbetaling);

			sumIndbetaling.Execute();
		}

		private DatabaseSumIndbetaling CreateDatabaseSumIndbetaling()
		{
			return new DatabaseSumIndbetaling()
			{
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};
		}
	}
}
