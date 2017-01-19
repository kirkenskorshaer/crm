using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataLayer.MongoData
{
	[BsonIgnoreExtraElements]
	public abstract class AbstractMongoData
	{
		public ObjectId _id { get; set; }

		[BsonIgnore]
		public string Id => _id.ToString();

		protected static List<TDataType> ReadById<TDataType>(MongoConnection connection, ObjectId id)
		where TDataType : AbstractMongoData
		{
			Expression<Func<TDataType, bool>> filter = data => data._id == id;

			IMongoCollection<TDataType> datas = connection.Database.GetCollection<TDataType>(typeof(TDataType).Name);
			IFindFluent<TDataType, TDataType> configFind = datas.Find(filter);
			Task<List<TDataType>> dataTask = configFind.ToListAsync();

			return dataTask.Result;
		}

		protected static TDataType ReadNextById<TDataType>(MongoConnection connection, ObjectId id)
		where TDataType : AbstractMongoData
		{
			Expression<Func<TDataType, bool>> filter = data => data._id > id;

			IMongoCollection<TDataType> datas = connection.Database.GetCollection<TDataType>(typeof(TDataType).Name);
			IFindFluent<TDataType, TDataType> find = datas.Find(filter);
			find.Limit(1);

			SortDefinitionBuilder<TDataType> sortBuilder = new SortDefinitionBuilder<TDataType>();
			SortDefinition<TDataType> sortDefinition = sortBuilder.Descending(document => document._id);

			find.Sort(sortDefinition);
			Task<TDataType> dataTask = find.SingleOrDefaultAsync();

			return dataTask.Result;
		}

		protected static TDataType First<TDataType>(MongoConnection connection)
		where TDataType : AbstractMongoData
		{
			return ReadNextById<TDataType>(connection, ObjectId.Empty);
		}

		protected TDataType Next<TDataType>(MongoConnection connection)
		where TDataType : AbstractMongoData
		{
			TDataType nextDocument = ReadNextById<TDataType>(connection, _id);

			if (nextDocument == null)
			{
				nextDocument = First<TDataType>(connection);
			}

			return nextDocument;
		}

		protected static void Create<TDataType>(MongoConnection connection, TDataType data)
		where TDataType : AbstractMongoData
		{
			data._id = ObjectId.GenerateNewId(DateTime.Now);
			IMongoCollection<TDataType> datas = connection.Database.GetCollection<TDataType>(typeof(TDataType).Name);
			Task insertTask = datas.InsertOneAsync(data);
			MongoDataHelper.WaitForTaskOrThrowTimeout(insertTask);
		}

		protected void Update<TDataType>(MongoConnection connection)
		where TDataType : AbstractMongoData
		{
			IMongoCollection<TDataType> datas = connection.Database.GetCollection<TDataType>(typeof(TDataType).Name);

			FilterDefinition<TDataType> filter = Builders<TDataType>.Filter.Eq(data => data._id, _id);

			Task<ReplaceOneResult> result = datas.ReplaceOneAsync(filter, (TDataType)this);
			result.Wait();
		}

		protected void Delete<TDataType>(MongoConnection connection)
		where TDataType : AbstractMongoData
		{
			IMongoCollection<TDataType> datas = connection.Database.GetCollection<TDataType>(typeof(TDataType).Name);
			Task deleteTask = datas.DeleteOneAsync(data => data._id == _id);
			deleteTask.Wait();
		}

		protected static long Count<TDataType>(MongoConnection connection)
		where TDataType : AbstractMongoData
		{
			IMongoCollection<TDataType> dataCollection = connection.Database.GetCollection<TDataType>(typeof(TDataType).Name);
			BsonDocument filter = new BsonDocument();
			Task<long> dataCount = dataCollection.CountAsync(filter);
			return MongoDataHelper.GetValueOrThrowTimeout(dataCount);
		}
	}
}
