using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DataLayer.MongoData.Option.Status
{
	public class ActiveOption
	{
		public string Name { get; set; }
		public string TypeName { get; set; }
		public ObjectId WorkerId { get; set; }

		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime Begin { get; set; }
	}
}
