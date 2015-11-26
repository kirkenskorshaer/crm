using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeToCrm : OptionBase
	{
		public Guid changeProviderId { get; set; }
		public string urlLoginName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SynchronizeToCrm>(connection);
			}
			else
			{
				Delete<SynchronizeToCrm>(connection);
			}
		}
	}
}
