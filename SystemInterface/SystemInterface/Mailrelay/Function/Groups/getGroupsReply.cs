using Newtonsoft.Json;

namespace SystemInterface.Mailrelay.Function.Groups
{
	public class getGroupsReply
	{
		public int id;
		public int position;
		public string name;
		public string description;
		public string enable;
		public string visible;
		[JsonProperty(PropertyName = "internal")]
		public string internalProperty;
	}
}
