using DataLayer.MongoData.Input;
using MongoDB.Bson;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class AddMailrelaySubscriberFromLead : OptionBase
	{
		public string urlLoginName { get; set; }
		public ObjectId WebCampaignId { get; set; }
		public string email { get; set; }

		public static AddMailrelaySubscriberFromLead Create(MongoConnection mongoConnection, WebCampaign webCampaign, string urlLoginName, string email, string name, Schedule schedule)
		{
			return Create(mongoConnection, webCampaign._id, urlLoginName, email, name, schedule);
		}

		public static AddMailrelaySubscriberFromLead Create(MongoConnection mongoConnection, ObjectId webCampaignId, string urlLoginName, string email, string name, Schedule schedule)
		{
			AddMailrelaySubscriberFromLead addMailrelaySubscriberFromLead = new AddMailrelaySubscriberFromLead()
			{
				WebCampaignId = webCampaignId,
				urlLoginName = urlLoginName,
				email = email,
			};

			Create(mongoConnection, addMailrelaySubscriberFromLead, name, schedule);

			return addMailrelaySubscriberFromLead;
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<AddMailrelaySubscriberFromLead>(connection);
			}
			else
			{
				Delete<AddMailrelaySubscriberFromLead>(connection);
			}
		}

		public byte[] WebCampaignIdValue()
		{
			return WebCampaignId.ToByteArray();
		}
	}
}
