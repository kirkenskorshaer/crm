using System;

namespace DataLayer.MongoData.Option.Options.Data.ContactChange
{
	public class ContactChangeRead : OptionBase
	{
		public Guid ContactId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ContactChangeRead>(connection);
			}
			else
			{
				Delete<ContactChangeRead>(connection);
			}
		}
	}
}