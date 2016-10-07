using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class Log
	{
		public ObjectId _id { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public string Location { get; set; }
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime CreatedTime { get; set; }
		public Config.LogLevelEnum LogLevel { get; set; }

		public static void Write(MongoConnection connection, string message, string location, string stackTrace, Config.LogLevelEnum logLevel)
		{
			Log log = new Log()
			{
				CreatedTime = DateTime.Now,
				Message = message,
				Location = location,
				StackTrace = stackTrace,
				LogLevel = logLevel,
			};

			log.Insert(connection);
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<Log> logs = connection.Database.GetCollection<Log>(typeof(Log).Name);
			Task insertTask = logs.InsertOneAsync(this);
			insertTask.Wait();
		}

		public static Log ReadLatest(MongoConnection connection)
		{
			IMongoCollection<Log> logs = connection.Database.GetCollection<Log>(typeof(Log).Name);

			SortDefinitionBuilder<Log> sortBuilder = new SortDefinitionBuilder<Log>();
			SortDefinition<Log> sortDefinition = sortBuilder.Descending(log => log.CreatedTime);

			IFindFluent<Log, Log> logFind = logs.Find(log => true).Sort(sortDefinition);
			Task<Log> logTask = logFind.FirstAsync();

			return logTask.Result;
		}
	}
}
