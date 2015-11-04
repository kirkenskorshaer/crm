using System;

namespace DataLayer.MongoData.Option.Options.Data.ExternalContact
{
	public class ExternalContactRead : OptionBase
	{
		public Guid ExternalContactId { get; set; }
		public Guid ChangeProviderId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ExternalContactRead>(connection);
			}
			else
			{
				Delete<ExternalContactRead>(connection);
			}
		}
	}
}