using System;

namespace SystemInterface.Mailrelay.Function.CustomFields
{
	public abstract class AbstractCustomField : AbstractFunction
	{
		public abstract override Type ReturnType { get; }

		public enum CustomFieldTypeEnum
		{
			TextField = 1,
			PasswordField = 2,
			Textarea = 3,
			SelectField = 4,
			Checkbox = 5,
			RadioButton = 6,
			Date = 7,
			BIrthday = 8,
		}
	}
}
