using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using DatabaseSumOptalt = DataLayer.MongoData.Option.Options.Logic.SumOptalt;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SumOptaltTest : TestBase
	{
		[Test]
		public void test()
		{
			DatabaseSumOptalt databaseSumOptalt = CreateDatabaseSumOptalt();

			SumOptalt sumOptalt = new SumOptalt(Connection, databaseSumOptalt);

			sumOptalt.Execute();
		}

		private DatabaseSumOptalt CreateDatabaseSumOptalt()
		{
			return new DatabaseSumOptalt()
			{
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
				campaignid = Guid.Parse("ff052597-5538-e611-80ef-001c4215c4a0"),
			};
		}
	}
}
