using System;

namespace DataLayer.MongoData.Option.Options.Data.ContactChange
{
	public class ContactChangeCreate : OptionBase
	{
		public DateTime CreatedOn { get; set; }
		public DateTime ModifiedOn { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }

		public Guid ExternalContactId { get; private set; }
		public Guid ChangeProviderId { get; private set; }
		public Guid ContactId { get; private set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ContactChangeCreate>(connection);
			}
			else
			{
				Delete<ContactChangeCreate>(connection);
			}
		}
	}
}