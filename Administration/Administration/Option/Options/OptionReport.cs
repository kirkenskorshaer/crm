using DataLayer.MongoData;
using System;
using System.Diagnostics;
using System.Text;

namespace Administration.Option.Options
{
	public class OptionReport
	{
		public Type OptionType;
		public int Workload;
		public int SubWorkload;
		public bool Success = false;
		public StringBuilder TextBuilder = new StringBuilder();
		public DateTime BeginTime;
		public DateTime EndTime;
		public int ProcessId;
		public long Memory;

		public OptionReport(Type name)
		{
			OptionType = name;
			BeginTime = DateTime.Now;
		}

		public void WriteLog(DataLayer.MongoConnection mongoConnection)
		{
			DataLayer.MongoData.Config.LogLevelEnum logLevel = DataLayer.MongoData.Config.LogLevelEnum.OptionReport;

			if (Success == false)
			{
				logLevel = DataLayer.MongoData.Config.LogLevelEnum.OptionError;
			}

			string finalReport = $"S:{Success} W:{Workload} SW:{SubWorkload}{Environment.NewLine}{TextBuilder.ToString()}";

			Log.Write(mongoConnection, finalReport, OptionType, logLevel);
		}

		public void FinishReport()
		{
			EndTime = DateTime.Now;

			Process currentProcess = Process.GetCurrentProcess();

			ProcessId = currentProcess.Id;
			Memory = currentProcess.WorkingSet64;
		}
	}
}
