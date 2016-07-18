using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.CustomFields
{
	public class addCustomField : AbstractCustomField
	{
		public override Type ReturnType { get { return typeof(MailrelayIntReply); } }

		///<summery>Name.</summery>
		public string name;

		///<summery>Field type.</summery>
		[ToGet(type = ToGetAttribute.typeEnum.intEnum)]
		public CustomFieldTypeEnum fieldType;

		///<summery>Position of the custom field on listing.</summery>
		public int position;

		///<summery>Default field value.</summery>
		[ToGet(name = "default")]
		public string defaultField;

		///<summery>True if field is enabled.</summery>
		public bool enable;
	}
}
