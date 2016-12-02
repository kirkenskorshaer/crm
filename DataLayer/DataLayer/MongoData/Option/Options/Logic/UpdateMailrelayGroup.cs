using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class UpdateMailrelayGroup : OptionBase
	{
		public string urlLoginName { get; set; }

		[BsonRepresentation(BsonType.String)]
		public Guid? listId { get; set; }

		public static UpdateMailrelayGroup Create(MongoConnection mongoConnection, string urlLoginName, string name, Guid? listId, Schedule schedule)
		{
			UpdateMailrelayGroup updateMailrelayGroup = new UpdateMailrelayGroup()
			{
				urlLoginName = urlLoginName,
				listId = listId,
			};

			Create(mongoConnection, updateMailrelayGroup, name, schedule);

			return updateMailrelayGroup;
		}
	}
}