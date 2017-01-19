using DataLayer;
using System;
using System.Threading;
using DatabaseTest = DataLayer.MongoData.Option.Options.Logic.Test;

namespace Administration.Option.Options.Logic
{
	public class Test : OptionBase
	{
		private DatabaseTest _databaseTest;

		public Test(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseTest = (DatabaseTest)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			TimeSpan sleepTime = TimeSpan.FromSeconds(_databaseTest.SleepForSeconds);

			Thread.Sleep(sleepTime);

			if (_databaseTest.ThrowException)
			{
				report.Success = false;

				throw new Exception($"Test exception {Guid.NewGuid()}");
			}

			report.Success = true;
		}
	}
}
