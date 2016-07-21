using System;
using System.Dynamic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getMailRcptInfo : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayObjectReply<getMailRcptInfoReply>); } }

		///<summery>Email address of the recipient.</summery>
		public string email;

		///<summery>Date to search in the following format: YYYY-MM-DD.</summery>
		[ToGet(type = ToGetAttribute.typeEnum.ShortDate)]
		public DateTime date;

		///<summery>ID of the message.</summery>
		public int id;
	}
}
