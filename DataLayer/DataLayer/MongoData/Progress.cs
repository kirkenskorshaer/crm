using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.MongoData
{
	public class Progress
	{
		public ObjectId _id { get; set; }

		public Guid TargetId { get; set; }
		public string TargetName { get; set; }

		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime LastProgressDate { get; set; }

		public static Progress ReadNext(MongoConnection connection, string targetName)
		{
			IMongoCollection<Progress> progressCollection = connection.Database.GetCollection<Progress>(typeof(Progress).Name);

			IFindFluent<Progress, Progress> progressFind = progressCollection.Find(progress => progress.TargetName == targetName);

			SortDefinition<Progress> sortDefinition = Builders<Progress>.Sort.Ascending(progress => progress.LastProgressDate);

			progressFind = progressFind.Sort(sortDefinition).Limit(1);

			Task<List<Progress>> progressTask = progressFind.ToListAsync();

			progressTask.Wait();

			List<Progress> progressFound = progressTask.Result;

			return progressFound.FirstOrDefault();
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<Progress> progressCollection = connection.Database.GetCollection<Progress>(typeof(Progress).Name);
			Task insertTask = progressCollection.InsertOneAsync(this);
			insertTask.Wait();
		}

		public void UpdateLastProgressDateToNow(MongoConnection connection)
		{
			LastProgressDate = DateTime.Now;

			IMongoCollection<Progress> progressCollection = connection.Database.GetCollection<Progress>(typeof(Progress).Name);

			FilterDefinition<Progress> filter = Builders<Progress>.Filter.Eq(progress => progress._id, _id);

			Task<ReplaceOneResult> result = progressCollection.ReplaceOneAsync(filter, this);
			result.Wait();
		}

		public void Delete(MongoConnection connection)
		{
			IMongoCollection<Progress> progressCollection = connection.Database.GetCollection<Progress>(typeof(Progress).Name);
			Task deleteTask = progressCollection.DeleteOneAsync(progress => progress._id == _id);
			deleteTask.Wait();
		}
	}
}
