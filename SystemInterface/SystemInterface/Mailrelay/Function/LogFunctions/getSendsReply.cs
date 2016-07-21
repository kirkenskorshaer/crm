using System;
using System.Collections.Generic;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getSendsReply
	{
		public string id;
		public string subscriber;
		public DateTime sent_date;
		public string campaign;
		public string result;
		public string spam_reported;
		public string result_reason;
		public string to;
		public List<string> smtp_tags;
	}
}
