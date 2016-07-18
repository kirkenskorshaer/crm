using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.CustomFields
{
	public class getCustomFields : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayArrayReply<getCustomFieldsReply>); } }

		///<summery>Optional parameter to specify offset.</summery>
		public int? offset;

		///<summery>Optional parameter to specify the number of records.</summery>
		public int? count;

		///<summery>Optional parameter to search by id.</summery>
		public int? id;

		///<summery>Optional parameter to search by name.</summery>
		public string name;

		///<summery>Optional parameter to search by field type.</summery>
		public int? fieldType;

		///<summery>Optional parameter to search if field is enabled.</summery>
		public bool? enable;

		///<summery>Optional parameter to specify a search field.</summery>
		public string sortField;

		///<summery>Optional parameter to specify a sort order. Accepted values are ASC and DESC (default).</summery>
		public sortOrderEnum? sortOrder;
	}
}
