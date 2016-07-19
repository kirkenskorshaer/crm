using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Groups
{
	public class deleteGroup : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		/// <summary>Id of the group that you want to delete.</summary>
		public int id;
	}
}
