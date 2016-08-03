using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemInterface.Mailrelay.Logic
{
	public class UpdateReport<ReportType>
	{
		public List<ReportType> Failed = new List<ReportType>();
		public List<ReportType> Updated = new List<ReportType>();
		public List<ReportType> AlreadyUpToDate = new List<ReportType>();

		public void CollectUpdate(UpdateResultEnum updateResult, ReportType reportValue)
		{
			switch (updateResult)
			{
				case UpdateResultEnum.Failed:
					Failed.Add(reportValue);
					break;
				case UpdateResultEnum.Updated:
					Updated.Add(reportValue);
					break;
				case UpdateResultEnum.AlreadyUpToDate:
					AlreadyUpToDate.Add(reportValue);
					break;
				default:
					throw new Exception($"unknown UpdateResult {updateResult}");
			}
		}

		public string AsLogText(string logTextName)
		{
			StringBuilder logTextBuilder = new StringBuilder();
			logTextBuilder.AppendLine(logTextName);
			logTextBuilder.AppendLine($"	Failed:{ListToLogText(Failed)}");
			logTextBuilder.AppendLine($"	Updated:{ListToLogText(Updated)}");
			logTextBuilder.AppendLine($"	AlreadyUpToDate:{ListToLogText(AlreadyUpToDate)}");

			return logTextBuilder.ToString();
		}

		private string ListToLogText(List<ReportType> reportList)
		{
			return string.Join(",", reportList);
		}
	}
}
