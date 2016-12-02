namespace DataLayer.MongoData.Option.Options.Logic
{
	public class AggregateSubTable : OptionBase
	{
		public AggregateEnum aggregate { get; set; }
		public string topEntityName { get; set; }
		public string topEntityIdName { get; set; }
		public string topEntityResultFieldName { get; set; }
		public string topEntityReferenceIdName { get; set; }
		public string aggregateReferenceIdName { get; set; }
		public string aggregateEntityName { get; set; }
		public string aggregateEntityIdName { get; set; }
		public string aggregateFieldName { get; set; }

		public enum AggregateEnum
		{
			sum = 1,
			count = 2,
		}
	}
}
