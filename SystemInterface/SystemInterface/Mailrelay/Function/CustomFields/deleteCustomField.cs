using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.CustomFields
{
	public class deleteCustomField : AbstractCustomField
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		/// <summary> Id of the custom field that you want to delete.</summary>
		public int id;
	}
}
