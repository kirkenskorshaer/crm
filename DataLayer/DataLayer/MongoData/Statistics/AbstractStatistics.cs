using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Statistics
{
	public abstract class AbstractStatistics
	{
		public ObjectId _id { get; set; }

		public string Name { get; set; }

		[BsonIgnore]
		public string Id => _id.ToString();


		protected static void Create<TStatisticsType>(MongoConnection connection, TStatisticsType statistics, string name)
		where TStatisticsType : AbstractStatistics
		{
			statistics._id = ObjectId.GenerateNewId(DateTime.Now);
			statistics.Name = name;

			IMongoCollection<TStatisticsType> options = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			Task insertTask = options.InsertOneAsync(statistics);
			insertTask.Wait();
		}

		protected static List<TStatisticsType> ReadById<TStatisticsType>(MongoConnection connection, ObjectId id)
		where TStatisticsType : AbstractStatistics
		{
			Expression<Func<TStatisticsType, bool>> filter = option => option._id == id;

			IMongoCollection<TStatisticsType> statistics = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			IFindFluent<TStatisticsType, TStatisticsType> statisticsFind = statistics.Find(filter);
			Task<List<TStatisticsType>> optionTask = statisticsFind.ToListAsync();

			return optionTask.Result;
		}

		protected void Update<TStatisticsType>(MongoConnection connection)
		where TStatisticsType : AbstractStatistics
		{
			IMongoCollection<TStatisticsType> statistics = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);

			FilterDefinition<TStatisticsType> filter = Builders<TStatisticsType>.Filter.Eq(option => option._id, _id);

			Task<ReplaceOneResult> result = statistics.ReplaceOneAsync(filter, (TStatisticsType)this);
			result.Wait();
		}

		protected void Delete<TStatisticsType>(MongoConnection connection)
		where TStatisticsType : AbstractStatistics
		{
			IMongoCollection<TStatisticsType> statistics = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			Task deleteTask = statistics.DeleteOneAsync(option => option._id == _id);
			deleteTask.Wait();
		}
	}
}
