using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class assignSubscribersToGroups : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayObjectReply<assignSubscribersToGroupsReply>); } }

		///<summery>An array with the id of the groups that you want to assign.</summery>
		public List<int> groups;

		///<summery>An array with the email of the subscribers that you want to assign.</summery>
		public List<string> subscribers;
	}
}
