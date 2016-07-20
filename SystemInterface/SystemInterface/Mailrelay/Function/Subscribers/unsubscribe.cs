using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class unsubscribe : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		/// <summary>Email of the subscriber that you want to unsubscribe.</summary>
		public string email;
	}
}
