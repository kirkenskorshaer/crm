using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Mailboxes
{
	public class getMailboxes : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<getMailboxesReply>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records.</summery>
		public int? count;

		///<summery>Optional parameter to search by id.</summery>
		public int? id;

		///<summery>Optional parameter to search by mailbox name.</summery>
		public string mailboxName;

		///<summery>Optional parameter to search by name.</summery>
		public string name;

		///<summery>Optional parameter to search by email.</summery>
		public string email;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public string sortOrder;
	}
}
