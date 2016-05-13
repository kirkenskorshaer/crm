using MongoDB.Bson;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeFromStub : OptionBase
	{
		public ObjectId WebCampaignId { get; set; }

		public byte[] WebCampaignIdValue()
		{
			return WebCampaignId.ToByteArray();
		}

		public static SynchronizeFromStub Create(MongoConnection mongoConnection, ObjectId webCampaignId, string name, Schedule schedule)
		{
			SynchronizeFromStub synchronizeFromStub = new SynchronizeFromStub()
			{
				WebCampaignId = webCampaignId,
			};

			Create(mongoConnection, synchronizeFromStub, name, schedule);

			return synchronizeFromStub;
		}

		protected override void Execute(MongoConnection mongoConnection, bool recurring)
		{
			if (recurring)
			{
				Update<SynchronizeFromStub>(mongoConnection);
			}
			else
			{
				Delete<SynchronizeFromStub>(mongoConnection);
			}
		}
	}
}
