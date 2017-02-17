using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class CopyNN : OptionBase
	{
		public string originIntermediateEntityName { get; set; }

		public string insertedEntityName { get; set; }
		public string insertedEntityIdName { get; set; }
		public string insertedEntityOriginIdName { get; set; }

		[BsonRepresentation(BsonType.String)]
		public Guid? insertedEntityId { get; set; }

		public string originEntityIdName { get; set; }
		public string targetName { get; set; }
		public string targetIdName { get; set; }
		public string intermediateEntityRelationshipName { get; set; }
	}
}
