using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.MongoData
{
	public class IntProgress
	{
		public ObjectId _id { get; set; }

		public Guid TargetId { get; set; }
		public string TargetName { get; set; }

		public int progressValue { get; set; }

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<IntProgress> IntprogressCollection = connection.Database.GetCollection<IntProgress>(typeof(IntProgress).Name);
			Task insertTask = IntprogressCollection.InsertOneAsync(this);
			insertTask.Wait();
		}

		public static bool Exists(MongoConnection connection, string targetName, Guid targetId)
		{
			IMongoCollection<IntProgress> IntprogressCollection = connection.Database.GetCollection<IntProgress>(typeof(IntProgress).Name);
			Task<long> IntprogressFind = IntprogressCollection.CountAsync(Intprogress =>
				Intprogress.TargetName == targetName &&
				Intprogress.TargetId == targetId);

			return IntprogressFind.Result > 0;
		}

		public static IntProgress Read(MongoConnection connection, string targetName, Guid targetId)
		{
			IMongoCollection<IntProgress> IntprogressCollection = connection.Database.GetCollection<IntProgress>(typeof(IntProgress).Name);
			IFindFluent<IntProgress, IntProgress> IntprogressFind = IntprogressCollection.Find(Intprogress =>
				Intprogress.TargetName == targetName &&
				Intprogress.TargetId == targetId);

			Task<List<IntProgress>> IntprogressTask = IntprogressFind.ToListAsync();

			IntprogressTask.Wait();

			List<IntProgress> IntprogressFound = IntprogressTask.Result;

			return IntprogressFound.Single();
		}

		public void Update(MongoConnection connection)
		{
			IMongoCollection<IntProgress> progressCollection = connection.Database.GetCollection<IntProgress>(typeof(IntProgress).Name);

			FilterDefinition<IntProgress> filter = Builders<IntProgress>.Filter.Eq(intProgress => intProgress._id, _id);

			Task<ReplaceOneResult> result = progressCollection.ReplaceOneAsync(filter, this);
			result.Wait();
		}

		public void Delete(MongoConnection connection)
		{
			IMongoCollection<IntProgress> IntprogressCollection = connection.Database.GetCollection<IntProgress>(typeof(IntProgress).Name);
			Task deleteTask = IntprogressCollection.DeleteOneAsync(Intprogress => Intprogress._id == _id);
			deleteTask.Wait();
		}
	}
}
