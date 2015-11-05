using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
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

		public static Progress ReadNext(string targetName)
		{
			
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<Progress> progressCollection = connection.Database.GetCollection<Progress>(typeof(Progress).Name);
			Task insertTask = progressCollection.InsertOneAsync(this);
			insertTask.Wait();
		}

		public void Delete(MongoConnection connection)
		{
			IMongoCollection<Progress> progressCollection = connection.Database.GetCollection<Progress>(typeof(Progress).Name);
			Task deleteTask = progressCollection.DeleteOneAsync(progress => progress._id == _id);
			deleteTask.Wait();
		}
	}
}
