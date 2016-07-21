using System;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getPackagesReply
	{
		public int id;
		public int contractId;
		public string type;
		public int softLimit;
		public int hardLimit;
		public string period;
		public DateTime? startDate;
		public string description;
		public DateTime? warningDate;
		public DateTime? hardWarningDate;
		public string warningPercentage;
		public string active;
		public int? subscribersLimit;
		public DateTime? subscribersLimitDate;
		public getPackagesReplyUsage usage;
	}
}
