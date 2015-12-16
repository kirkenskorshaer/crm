using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Statistics
{
	public abstract class AbstractDateTimeStatistics : AbstractStatistics
	{
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime XValue { get; set; }
	}
}
