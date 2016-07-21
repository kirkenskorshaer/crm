using System;
using System.Collections.Generic;
using System.Dynamic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getSends : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<getSendsReply>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records. If not specified, it's 100. The maximum allowed value is 1000. Numbers bigger than 1000 are automatically reduced to 1000.</summery>
		public int? count;

		///<summery>Optional parameter to search for rcpt's email.</summery>
		public string email;

		///<summery>Optional parameter for start date. Must be in the following format: YYYY-MM-DD HH:MM:SS.</summery>
		public string startDate;

		///<summery>Optional parameter for end date. Must be in the following format: YYYY-MM-DD HH:MM:SS.</summery>
		public string endDate;

		///<summery>Optional parameter by mailing list.</summery>
		public int? mailingList;

		///<summery>Optional parameter by result. Valid values are: "b" (bounced), "d" (delivered), "i" (ignored) and "s" (soft bounced).</summery>
		public string result;

		///<summery>Optional parameter to search for messages reported as spam.</summery>
		public bool? spamReported;

		///<summery>Optional parameter to for messages in specific smtp tags.</summery>
		public List<string> smtpTags;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public string sortOrder;
	}
}
