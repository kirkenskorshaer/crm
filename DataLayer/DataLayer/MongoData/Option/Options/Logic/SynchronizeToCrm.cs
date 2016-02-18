using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeToCrm : OptionBase
	{
		public Guid changeProviderId { get; set; }
		public string urlLoginName { get; set; }
		public SynchronizeTypeEnum synchronizeType { get; set; }

		public enum SynchronizeTypeEnum
		{
			Contact = 1,
			Account = 2,
			Both = 3,
		}


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
