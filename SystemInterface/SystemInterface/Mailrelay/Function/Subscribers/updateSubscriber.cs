using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class updateSubscriber : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		///<summery>Id of the subscriber that you want to update.</summery>
		public int id;

		///<summery>Email.</summery>
		public string email;

		///<summery>Name.</summery>
		public string name;

		///<summery>Array with the IDs of the groups that this subscriber belongs. Example for groups 10 and 14: array( 10, 14 )</summery>
		public List<int> groups;

		///<summery>Optional parameter for custom fields. Use f_FieldId as key. Example with two fields: array( 'f_1' => 'Madrid', 'f_2' => '5555-5555' )</summery>
		public Dictionary<string, string> customFields;

		///<summery>Optional parameter to activate/inactive subscriber</summery>
		public bool? activated;
	}
}
