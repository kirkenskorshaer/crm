using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class updateSubscribers : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		///<summery>Array of subscriber ids.</summery>
		public List<int> ids;

		///<summery>Optional parameter to activate/inactive subscribers.</summery>
		public bool activated;

		///<summery>Optional parameter to ban subscribers.</summery>
		public bool banned;

		///<summery>Optional parameter set subscribers as deleted.</summery>
		public bool deleted;
	}
}
