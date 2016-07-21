using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getPackages : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<getPackagesReply>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records.</summery>
		public int? count;

		///<summery>Optional parameter to search by id.</summery>
		public int? id;

		///<summery>Optional parameter to search by name.</summery>
		public string name;

		///<summery>Start date for package's usage info. If not specified, will be current billing cycle.</summery>
		public string usageStartDate;

		///<summery>End date for package's usage info. If not specified, will be today.</summery>
		public string usageEndDate;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public sortOrderEnum sortOrder;
	}
}
