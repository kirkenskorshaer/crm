using System;

namespace DataLayer.MongoData.Option.Options.Data.ChangeProvider
{
	public class ChangeProviderRead : OptionBase
	{
		public Guid ChangeProviderId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ChangeProviderRead>(connection);
			}
			else
			{
				Delete<ChangeProviderRead>(connection);
			}
		}
	}
}