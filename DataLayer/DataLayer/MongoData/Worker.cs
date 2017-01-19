using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLayer.MongoData
{
	public class Worker : AbstractMongoData
	{
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime LastWorkTime { get; set; }

		public void Create(MongoConnection connection)
		{
			_id = ObjectId.GenerateNewId(DateTime.Now);
			LastWorkTime = DateTime.Now;

			IMongoCollection<Worker> datas = connection.Database.GetCollection<Worker>(typeof(Worker).Name);
			Task insertTask = datas.InsertOneAsync(this);
			MongoDataHelper.WaitForTaskOrThrowTimeout(insertTask);
		}

		public static List<Worker> GetDeadWorkers(MongoConnection mongoConnection, DateTime lastAcceptableWorkerTime)
		{
			IMongoCollection<Worker> collection = mongoConnection.Database.GetCollection<Worker>(typeof(Worker).Name);

			FilterDefinition<Worker> filter = Builders<Worker>.Filter.Lt(worker => worker.LastWorkTime, lastAcceptableWorkerTime);

			IFindFluent<Worker, Worker> find = collection.Find(filter);

			Task<List<Worker>> task = find.ToListAsync();

			return MongoDataHelper.GetValueOrThrowTimeout(task);
		}

		public void Delete(MongoConnection mongoConnection)
		{
			Delete<Worker>(mongoConnection);
		}

		public void Update(MongoConnection mongoConnection)
		{
			Update<Worker>(mongoConnection);
		}
	}
}
