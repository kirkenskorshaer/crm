using System;

namespace DataLayer.MongoData.Option.Options.Data.Contact
{
	public class ContactRead : OptionBase
	{
		public Guid ContactId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ContactRead>(connection);
			}
			else
			{
				Delete<ContactRead>(connection);
			}
		}
	}
}