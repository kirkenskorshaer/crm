using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SumOptalt : OptionBase
	{
		public string urlLoginName { get; set; }
		public decimal? kreds { get; set; }
		public decimal? by { get; set; }
		public decimal? total { get; set; }

		[BsonRepresentation(BsonType.String)]
		public Guid campaignid { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SumOptalt>(connection);
			}
			else
			{
				Delete<SumOptalt>(connection);
			}
		}
	}
}
