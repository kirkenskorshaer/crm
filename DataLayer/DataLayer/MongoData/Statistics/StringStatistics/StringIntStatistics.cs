using MongoDB.Bson;
using System.Collections.Generic;

namespace DataLayer.MongoData.Statistics.StringStatistics
{
	public class StringIntStatistics : AbstractStringStatistics
	{
		public int YValue { get; set; }

		public static StringIntStatistics Create(MongoConnection connection, string name, string x, int y)
		{
			StringIntStatistics intTimespanStatistics = new StringIntStatistics
			{
				XValue = x,
				YValue = y,
			};

			Create(connection, intTimespanStatistics, name);

			return intTimespanStatistics;
		}

		public static List<StringIntStatistics> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<StringIntStatistics>(connection, objectId);
		}
	}
}
