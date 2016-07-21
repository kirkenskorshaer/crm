using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Campaigns
{
	public class getCampaigns : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<getCampaignsReply>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records.</summery>
		public int? count;

		///<summery>Optional parameter to search by id.</summery>
		public int? id;

		///<summery>Optional parameter to search by subject.</summery>
		public string subject;

		///<summery>Optional parameter to search deleted campaigns</summery>
		public bool? deleted;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public sortOrderEnum sortOrder;
	}
}
