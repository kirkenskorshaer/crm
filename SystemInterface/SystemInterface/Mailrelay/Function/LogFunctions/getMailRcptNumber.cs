using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getMailRcptNumber : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayIntReply); } }

		///<summery>Email address of the recipient.</summery>
		public string email;

		///<summery>Date to search in the following format: YYYY-MM-DD.</summery>
		[ToGet(type = ToGetAttribute.typeEnum.ShortDate)]
		public DateTime date;
	}
}
