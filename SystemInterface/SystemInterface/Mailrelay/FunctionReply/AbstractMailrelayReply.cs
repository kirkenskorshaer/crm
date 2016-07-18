using Newtonsoft.Json;

namespace SystemInterface.Mailrelay.FunctionReply
{
	public abstract class AbstractMailrelayReply
	{
		public int status;
		public string error;

		public string ToJson()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
			};

			return JsonConvert.SerializeObject(this, settings);
		}
	}
}
