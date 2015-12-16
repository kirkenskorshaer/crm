using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace DataLayer.MongoData.Statistics.IntStatistics
{
	public class IntTimespanStatistics : AbstractIntStatistics
	{
		public int YValue { get; set; }
		public string ZValue { get; set; }

		public static IntTimespanStatistics Create(MongoConnection connection, string name, int x, int y)
		{
			IntTimespanStatistics intTimespanStatistics = new IntTimespanStatistics
			{
				XValue = x,
				YValue = y,
				ZValue = x.ToString(),
			};

			Create(connection, intTimespanStatistics, name);

			return intTimespanStatistics;
		}

		public static List<IntTimespanStatistics> Read(MongoConnection connection, string id)
		{
			ObjectId objectId = new ObjectId(id);
			return ReadById<IntTimespanStatistics>(connection, objectId);
		}
	}
}
