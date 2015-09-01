using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DataLayer.MongoData.Option
{
	public abstract class OptionBase
	{
		public ObjectId _id { get; set; }
		public Schedule Schedule { get; set; }
		public string Name { get; set; }

		[BsonIgnore]
		public string Id => _id.ToString();

		protected static void Create<TOptionType>(MongoConnection connection, TOptionType option, string name, Schedule schedule)
		where TOptionType : OptionBase
		{
			option._id = ObjectId.GenerateNewId(DateTime.Now);
			option.Name = name;
			option.Schedule = schedule;

			IMongoCollection<TOptionType> options = connection.Database.GetCollection<TOptionType>(typeof(TOptionType).Name);
			Task insertTask = options.InsertOneAsync(option);
			insertTask.Wait();
		}

		protected static List<TOptionType> Read<TOptionType>(MongoConnection connection, Expression<Func<TOptionType, bool>> filter)
		{
			IMongoCollection<TOptionType> options = connection.Database.GetCollection<TOptionType>(typeof(TOptionType).Name);
			IFindFluent<TOptionType, TOptionType> configFind = options.Find(filter);
			Task<List<TOptionType>> optionTask = configFind.ToListAsync();

			return optionTask.Result;
		}

		protected void Update<TOptionType>(MongoConnection connection)
		where TOptionType : OptionBase
		{
			IMongoCollection<TOptionType> options = connection.Database.GetCollection<TOptionType>(typeof(TOptionType).Name);

			FilterDefinition<TOptionType> filter = Builders<TOptionType>.Filter.Eq(option => option._id, _id);

			Task<ReplaceOneResult> result = options.ReplaceOneAsync(filter, (TOptionType)this);
			result.Wait();
		}

		protected void Delete<TOptionType>(MongoConnection connection)
		where TOptionType : OptionBase
		{
			IMongoCollection<TOptionType> options = connection.Database.GetCollection<TOptionType>(typeof(TOptionType).Name);
			Task deleteTask = options.DeleteOneAsync(option => option._id == _id);
			deleteTask.Wait();
		}
	}
}
