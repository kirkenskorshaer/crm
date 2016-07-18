using System.Collections.Generic;

namespace SystemInterface.Mailrelay.FunctionReply
{
	public class MailrelayArrayReply<DataType> : AbstractMailrelayReply
	{
		public List<DataType> data;
	}
}
