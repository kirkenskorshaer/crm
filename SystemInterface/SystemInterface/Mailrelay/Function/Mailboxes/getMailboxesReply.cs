using Newtonsoft.Json;

namespace SystemInterface.Mailrelay.Function.Mailboxes
{
	public class getMailboxesReply
	{
		public int id;
		public string mailbox_name;
		public string name;
		public string email;
		public string check;
		public string username;
		public string password;
		public string hostname;
		public string imap;
		public string apop;
		public string delete;
		public string enable;
		public string confirmed;
		[JsonProperty(PropertyName = "internal")]
		public string internalProperty;
	}
}
