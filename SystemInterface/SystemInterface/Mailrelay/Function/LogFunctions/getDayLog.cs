using System;
using System.Dynamic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getDayLog : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<ExpandoObject>); } }

		///<summery>Date in the following format: YYYY-MM-DD.</summery>
		[ToGet(type = ToGetAttribute.typeEnum.ShortDate)]
		public DateTime date;

		///<summery>Optional parameter to specify offset. If not specified, it's 0.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records. If not specified, it's 100. The maximum allowed value is 1000. Numbers bigger than 1000 are automatically reduced to 1000.</summery>
		public int? count;
	}
}
