using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeFromCrm : OptionBase
	{
		public Guid changeProviderId { get; set; }
		public string urlLoginName { get; set; }

		public static SynchronizeFromCrm Create(MongoConnection connection, string name, Schedule schedule, string urlLoginName, Guid changeProviderId)
		{
			SynchronizeFromCrm synchronizeFromCrm = new SynchronizeFromCrm
			{
				changeProviderId = changeProviderId,
				urlLoginName = urlLoginName,
			};

			Create(connection, synchronizeFromCrm, name, schedule);

			return synchronizeFromCrm;
		}

		public void Delete(MongoConnection connection)
		{
			Delete<SynchronizeFromCrm>(connection);
		}

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
