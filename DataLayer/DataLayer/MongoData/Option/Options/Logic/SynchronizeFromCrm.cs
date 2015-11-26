using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeFromCrm : OptionBase
	{
		public Guid changeProviderId { get; set; }
		public string urlLoginName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SynchronizeFromCrm>(connection);
			}
			else
			{
				Delete<SynchronizeFromCrm>(connection);
			}
		}
	}
}
