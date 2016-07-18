using Newtonsoft.Json;

namespace SystemInterface.Mailrelay.Function.CustomFields
{
	public class getCustomFieldsReply
	{
		public string id;
		public string position;
		public string name;
		public string options;
		[JsonProperty(PropertyName = "default")]
		public string defaultProperty;
		public string enable;
		public string field_type;
	}
}
