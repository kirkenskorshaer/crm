using System;

namespace DataLayer.MongoData.Option.Options.Data.ExternalContact
{
	public class ExternalContactCreate : OptionBase
	{
		public Guid ExternalContactId { get; set; }
		public Guid ChangeProviderId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ExternalContactCreate>(connection);
			}
			else
			{
				Delete<ExternalContactCreate>(connection);
			}
		}
	}
}