using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class ExportContactToMailrelay : OptionBase
	{
		public string urlLoginName { get; set; }

		[BsonRepresentation(BsonType.String)]
		public Guid? listId { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ExportContactToMailrelay>(connection);
			}
			else
			{
				Delete<ExportContactToMailrelay>(connection);
			}
		}
	}
}
