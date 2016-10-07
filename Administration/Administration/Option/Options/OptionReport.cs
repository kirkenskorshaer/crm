using System;
using System.Text;

namespace Administration.Option.Options
{
	public class OptionReport
	{
		public string Name;
		public int Workload;
		public int SubWorkload;
		public bool Success = false;
		public StringBuilder TextBuilder = new StringBuilder();

		public OptionReport(string name)
		{
			Name = name;
		}

		public void WriteLog(DataLayer.MongoConnection mongoConnection)
		{
			DataLayer.MongoData.Config.LogLevelEnum logLevel = DataLayer.MongoData.Config.LogLevelEnum.OptionReport;

			if (Success == false)
			{
				logLevel = DataLayer.MongoData.Config.LogLevelEnum.OptionError;
			}

			string finalReport = $"S:{Success} W:{Workload} SW:{SubWorkload}{Environment.NewLine}{TextBuilder.ToString()}";

			Log.WriteLocation(mongoConnection, finalReport, Name, logLevel);
		}
	}
}
