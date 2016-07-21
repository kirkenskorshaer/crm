using System;
using System.Collections.Generic;

namespace SystemInterface.Mailrelay.Function.Campaigns
{
	public class getCampaignsReply
	{
		public int id;
		public string subject;
		public int mailbox_from_id;
		public int mailbox_reply_id;
		public int mailbox_report_id;
		public string groups;
		public string text;
		public string html;
		public string attachs;
		public DateTime date;
		public DateTime created;
		public DateTime? last_sent;
		public string deleted;
		public DateTime? send_date;
		public int subscribers_total;
		public int package_id;
		public int id_campaign_folder;
		public string analytics_utm_campaign;
		public List<int> mailing_lists;
	}
}