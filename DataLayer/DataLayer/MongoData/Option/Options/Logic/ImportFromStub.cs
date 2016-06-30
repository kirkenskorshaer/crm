using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataLayer.MongoData.Input;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class ImportFromStub : OptionBase
	{
		public string urlLoginName { get; set; }
		public ObjectId WebCampaignId { get; set; }

		public static ImportFromStub Create(MongoConnection mongoConnection, ObjectId webCampaignId, string urlLoginName, string name, Schedule schedule)
		{
			ImportFromStub importFromStub = new ImportFromStub()
			{
				WebCampaignId = webCampaignId,
				urlLoginName = urlLoginName,
			};

			Create(mongoConnection, importFromStub, name, schedule);

			return importFromStub;
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ImportFromStub>(connection);
			}
			else
			{
				Delete<ImportFromStub>(connection);
			}
		}

		public static List<ImportFromStub> ReadByWebCampaign(MongoConnection connection, WebCampaign webCampaign)
		{
			return ReadByWebCampaignId(connection, webCampaign._id);
		}

		private static List<ImportFromStub> ReadByWebCampaignId(MongoConnection connection, ObjectId WebCampaignId)
		{
			Expression<Func<ImportFromStub, bool>> filter = option => option.WebCampaignId == WebCampaignId;

			IMongoCollection<ImportFromStub> options = connection.Database.GetCollection<ImportFromStub>(typeof(ImportFromStub).Name);
			IFindFluent<ImportFromStub, ImportFromStub> configFind = options.Find(filter);
			Task<List<ImportFromStub>> optionTask = configFind.ToListAsync();

			return optionTask.Result;
		}

		public byte[] WebCampaignIdValue()
		{
			return WebCampaignId.ToByteArray();
		}
	}
}
