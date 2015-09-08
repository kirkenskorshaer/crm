﻿using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class Log
	{
		private static readonly string name = "log";

		public ObjectId _id { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime CreatedTime { get; set; }

		public static void Write(MongoConnection connection, string message, string stackTrace)
		{
			Log log = new Log()
			{
				CreatedTime = DateTime.Now,
				Message = message,
				StackTrace = stackTrace,
			};

			IMongoCollection<Log> logs = connection.Database.GetCollection<Log>(name);
			Task insertTask = logs.InsertOneAsync(log);
			insertTask.Wait();
		}

		public static Log ReadLatest(MongoConnection connection)
		{
			IMongoCollection<Log> logs = connection.Database.GetCollection<Log>(name);

			SortDefinitionBuilder<Log> sortBuilder = new SortDefinitionBuilder<Log>();
			SortDefinition<Log> sortDefinition = sortBuilder.Descending(log => log.CreatedTime);
			
			IFindFluent<Log, Log> logFind = logs.Find(log => true).Sort(sortDefinition);
			Task<Log> logTask = logFind.FirstAsync();

			return logTask.Result;
		}
	}
}