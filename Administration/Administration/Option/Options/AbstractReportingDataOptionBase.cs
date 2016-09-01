using Administration.Option.Options.Data;
using System;
using DataLayer;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options
{
	public abstract class AbstractReportingDataOptionBase : AbstractDataOptionBase
	{
		public AbstractReportingDataOptionBase(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		protected override bool ExecuteOption()
		{
			OptionReport report = new OptionReport(GetType().Name);

			try
			{
				ExecuteOption(report);
				Log.Write(Connection, report.ToString(), DataLayer.MongoData.Config.LogLevelEnum.OptionReport);
			}
			catch (Exception)
			{
				Log.Write(Connection, report.ToString(), DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				throw;
			}

			return report.Success;
		}

		protected abstract void ExecuteOption(OptionReport report);
	}
}
