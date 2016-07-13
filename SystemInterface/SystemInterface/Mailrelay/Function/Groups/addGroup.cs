using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Groups
{
	public class addGroup : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayIntReply); } }

		///<summery>Name.</summery>
		public string name;

		///<summery>Description.</summery>
		public string description;

		///<summery>Position of the group on listing.</summery>
		public int position;

		///<summery>True if group is enabled.</summery>
		public bool enable;

		///<summery>True if group is visible.</summery>
		public bool visible;
	}
}
