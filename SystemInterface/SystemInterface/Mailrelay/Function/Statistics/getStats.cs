using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Statistics
{
	public class getStats : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayObjectReply<getStatsReply>); } }

		/// <summary>Optional parameter if you want to get stats for a single mailing list.If not set, will return for all mailing lists.</summary>
		public int? id;

		/// <summary>Optional parameter for start date.Must be in the following format: YYYY-MM-DD HH:MM:SS.</summary>
		public DateTime? startDate;

		/// <summary>Optional parameter for end date.Must be in the following format: YYYY-MM-DD HH:MM:SS.</summary>
		public DateTime? endDate;
	}
}
