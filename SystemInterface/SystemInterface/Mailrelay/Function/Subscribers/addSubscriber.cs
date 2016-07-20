using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.Subscribers
{
	public class addSubscriber : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayIntReply); } }

		/// <summary>Email.</summary>
		public string email;

		/// <summary>Name.</summary>
		public string name;

		/// <summary>Array with the IDs of the groups that this subscriber belongs.
		/// Example for groups 10 and 14:
		/// array( 10, 14 );</summary>
		public List<int> groups;

		/// <summary>Optional parameter for custom fields. Use f_FieldId as key.
		/// Example with two fields:
		/// array( 'f_1' => 'Madrid', 'f_2' => '5555-5555' )</summary>
		public Dictionary<string, string> customFields;
	}
}
