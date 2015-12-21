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

			IMongoCollection<TStatisticsType> statisticsCollection = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			Task insertTask = statisticsCollection.InsertOneAsync(statistics);
			insertTask.Wait();
		}

		protected static List<TStatisticsType> ReadById<TStatisticsType>(MongoConnection connection, ObjectId id)
		where TStatisticsType : AbstractStatistics
		{
			Expression<Func<TStatisticsType, bool>> filter = statistics => statistics._id == id;

			IMongoCollection<TStatisticsType> statisticsCollection = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			IFindFluent<TStatisticsType, TStatisticsType> statisticsFind = statisticsCollection.Find(filter);
			Task<List<TStatisticsType>> optionTask = statisticsFind.ToListAsync();

			return optionTask.Result;
		}

		protected static List<TStatisticsType> ReadByName<TStatisticsType>(MongoConnection connection, string name)
		where TStatisticsType : AbstractStatistics
		{
			Expression<Func<TStatisticsType, bool>> filter = statistics => statistics.Name == name;

			IMongoCollection<TStatisticsType> statisticsCollection = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			IFindFluent<TStatisticsType, TStatisticsType> statisticsFind = statisticsCollection.Find(filter);
			Task<List<TStatisticsType>> optionTask = statisticsFind.ToListAsync();

			return optionTask.Result;
		}


		protected void Update<TStatisticsType>(MongoConnection connection)
		where TStatisticsType : AbstractStatistics
		{
			IMongoCollection<TStatisticsType> statisticsCollection = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);

			FilterDefinition<TStatisticsType> filter = Builders<TStatisticsType>.Filter.Eq(statistics => statistics._id, _id);

			Task<ReplaceOneResult> result = statisticsCollection.ReplaceOneAsync(filter, (TStatisticsType)this);
			result.Wait();
		}

		protected void Delete<TStatisticsType>(MongoConnection connection)
		where TStatisticsType : AbstractStatistics
		{
			IMongoCollection<TStatisticsType> statisticsCollection = connection.Database.GetCollection<TStatisticsType>(typeof(TStatisticsType).Name);
			Task deleteTask = statisticsCollection.DeleteOneAsync(statistics => statistics._id == _id);
			deleteTask.Wait();
		}
	}
}
