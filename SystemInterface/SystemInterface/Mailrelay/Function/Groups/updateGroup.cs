using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Groups
{
	public class updateGroup : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		///<summery>Id of the group that you want to update.</summery>
		public int id;

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