using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class getSubscribers : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<getSubscribersReply>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records.</summery>
		public int? count;

		///<summery>Optional parameter to search by id.</summery>
		public int? id;

		///<summery>Optional parameter to search by email.</summery>
		public string email;

		///<summery>Optional parameter to search by name.</summery>
		public string name;

		///<summery>Optional parameter to search by groups. You can specify one or more ids. Example: array( 1, 4 )</summery>
		public List<int> groups;

		///<summery>Optional parameter to search deleted subscribers.</summery>
		public bool? deleted;

		///<summery>Optional parameter to search activated field.</summery>
		public bool? activated;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public string sortOrder;

		///<summery>Optional parameter to only get subscribers that were marked as bounced.</summery>
		public bool? bounced;

		///<summery>Optional parameter to search by bounced date. It only works if bounced parameter is true.</summery>
		public string startBouncedDate;

		///<summery>Optional parameter to search by bounced date. It only works if bounced parameter is true.</summery>
		public string endBouncedDate;

		///<summery>Optional parameter to only get subscribers that reported your messages as spam.</summery>
		public bool? spamReported;

		///<summery>Optional parameter to only get subscribers that opted out.</summery>
		public bool? optedOut;

		///<summery>Optional parameter to only get subscribers that opted in.</summery>
		public bool? optedIn;
	}
}
