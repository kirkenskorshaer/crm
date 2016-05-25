using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Input
{
	public class Stub
	{
		public ObjectId _id { get; set; }
		public List<StubElement> Contents = new List<StubElement>();
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime PostTime;
		public ObjectId WebCampaignId;
		public int ImportAttempt = 0;

		public void Push(MongoConnection connection)
		{
			IMongoCollection<Stub> stubCollection = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			Task insertTask = stubCollection.InsertOneAsync(this);
			MongoDataHelper.WaitForTaskOrThrowTimeout(insertTask);
		}

		public static Stub ReadFirst(MongoConnection connection)
		{
			SortDefinition<Stub> sortDefinition = Builders<Stub>.Sort.Ascending("ImportAttempt").Ascending("PostTime");

			BsonDocument filter = new BsonDocument();

			IMongoCollection<Stub> stubCollection = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			IFindFluent<Stub, Stub> stubFind = stubCollection.Find(filter).Sort(sortDefinition).Limit(1);

			Task<Stub> stubTask = stubFind.FirstOrDefaultAsync();
			return MongoDataHelper.GetValueOrThrowTimeout(stubTask);
		}

		public static Stub ReadFirst(MongoConnection connection, WebCampaign webCampaign)
		{
			SortDefinition<Stub> sortDefinition = Builders<Stub>.Sort.Ascending("ImportAttempt").Ascending("PostTime");

			Expression<Func<Stub, bool>> filter = option =>	option.WebCampaignId == webCampaign._id;

			IMongoCollection<Stub> stubCollection = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			IFindFluent<Stub, Stub> stubFind = stubCollection.Find(filter).Sort(sortDefinition).Limit(1);

			Task<Stub> stubTask = stubFind.FirstOrDefaultAsync();
			return MongoDataHelper.GetValueOrThrowTimeout(stubTask);
		}

		public static long Count(MongoConnection connection)
		{
			IMongoCollection<Stub> stubCollection = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			BsonDocument filter = new BsonDocument();
			Task<long> stubCount = stubCollection.CountAsync(filter);
			return MongoDataHelper.GetValueOrThrowTimeout(stubCount);
		}

		public void Delete(MongoConnection connection)
		{
			IMongoCollection<Stub> stubCollection = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			Task deleteTask = stubCollection.DeleteOneAsync(stub => stub._id == _id);
			MongoDataHelper.WaitForTaskOrThrowTimeout(deleteTask);
		}

		public void Update(MongoConnection connection)
		{
			IMongoCollection<Stub> stubs = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			FilterDefinition<Stub> filter = Builders<Stub>.Filter.Eq(stub => stub._id, _id);
			Task<ReplaceOneResult> result = stubs.ReplaceOneAsync(filter, this);
			result.Wait();
		}
	}
}
