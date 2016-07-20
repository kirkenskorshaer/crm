using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class getSubscribersReply
	{
		public string id;
		public string name;
		public string email;
		public Dictionary<string, string> fields;
		public string code;
		public string banned;
		public string activated;
		public string deleted;
		public DateTime? date;
		[JsonProperty()]
		public DateTime? updated_date;
		public DateTime? optout_date;
		public string optout_ip;
		public DateTime? optin_date;
		public string optin_ip;
		public List<string> optout_mailing_list;
		public int? score;
		public List<string> groups;
	}
}
