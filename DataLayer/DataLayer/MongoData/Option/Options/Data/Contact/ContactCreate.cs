using System;

namespace DataLayer.MongoData.Option.Options.Data.Contact
{
	public class ContactCreate : OptionBase
	{
		public DateTime CreatedOn { get; set; }
		public DateTime ModifiedOn { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public Guid ContactId { get; private set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ContactCreate>(connection);
			}
			else
			{
				Delete<ContactCreate>(connection);
			}
		}
	}
}