using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Input
{
	public class Stub : AbstractMongoData
	{
		public List<StubElement> Contents = new List<StubElement>();
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime PostTime;
		public ObjectId WebCampaignId;
		public int ImportAttempt = 0;

		public void Push(MongoConnection connection)
		{
			Create(connection, this);
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

			Expression<Func<Stub, bool>> filter = option => option.WebCampaignId == webCampaign._id;

			IMongoCollection<Stub> stubCollection = connection.Database.GetCollection<Stub>(typeof(Stub).Name);
			IFindFluent<Stub, Stub> stubFind = stubCollection.Find(filter).Sort(sortDefinition).Limit(1);

			Task<Stub> stubTask = stubFind.FirstOrDefaultAsync();
			return MongoDataHelper.GetValueOrThrowTimeout(stubTask);
		}

		public static long Count(MongoConnection connection)
		{
			return Count<Stub>(connection);
		}

		public void Delete(MongoConnection connection)
		{
			Delete<Stub>(connection);
		}

		public void Update(MongoConnection connection)
		{
			Update<Stub>(connection);
		}
	}
}