using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay
{
	public interface IMailrelayConnection
	{
		AbstractMailrelayReply Send(AbstractFunction functionToSend);
	}
}
