using System;
using System.Dynamic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.SMTP
{
	public class getSmtpTags : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<ExpandoObject>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int offset;

		///<summery>Optional parameter to specify the number of records.</summery>
		public int? count;

		///<summery>Optional parameter to search by id.</summery>
		public int? id;

		///<summery>Optional parameter to search by tag.</summery>
		public string tag;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public string sortOrder;
	}
}
