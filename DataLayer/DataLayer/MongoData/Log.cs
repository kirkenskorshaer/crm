using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public static Config.LogLevelEnum WriteLogLevel = Config.LogLevelEnum.HeartError | Config.LogLevelEnum.OptionError;
		private static List<Log> _logCache = new List<Log>();
		private static object _cacheLock = new object();
		public static int MaxLookbackCacheSize = 20;
		public static int MaxSecondsToDiscardIdenticalLogMessages = 60 * 5;
		public static object fileLock = new object();

		public static void FileWrite(string location, string message)
		{
			string filename = $"C:/kkSystem/log/log_{DateTime.Now.ToString("yyyy_MM_dd")}.txt";

			lock (fileLock)
			{
				File.AppendAllText(filename, $"[{location}] [{DateTime.Now.ToString("HH:mm:ss")}] {message}{Environment.NewLine}");
			}
		}

		public static void Write(MongoConnection connection, string message, Config.LogLevelEnum logLevel)
		{
			WriteLocation(connection, message, string.Empty, string.Empty, logLevel);
		}

		public static void WriteLocation(MongoConnection connection, string message, string location, Config.LogLevelEnum logLevel)
		{
			WriteLocation(connection, message, location, string.Empty, logLevel);
		}

		public static void Write(MongoConnection connection, string message, string stackTrace, Config.LogLevelEnum logLevel)
		{
			WriteLocation(connection, message, string.Empty, string.Empty, logLevel);
		}

		public static void WriteLocation(MongoConnection connection, string message, string location, string stackTrace, Config.LogLevelEnum logLevel)
		{
			if (WriteLogLevel.HasFlag(logLevel))
			{
				Log log = new Log()
				{
					CreatedTime = DateTime.Now,
					Message = message,
					Location = location,
					StackTrace = stackTrace,
					LogLevel = logLevel,
				};

				if (IsRepeatedMessage(log) == false)
				{
					log.Insert(connection);
				}
			}
		}

		private static bool IsRepeatedMessage(Log log)
		{
			lock (_cacheLock)
			{
				if (_logCache.Any(cachedLog => IsSameAndNew(cachedLog, log)))
				{
					return true;
				}

				if (_logCache.Count >= MaxLookbackCacheSize)
				{
					_logCache.RemoveAt(0);
				}

				_logCache.Add(log);

				return false;
			}
		}

		private static bool IsSameAndNew(Log cachedLog, Log log)
		{
			return
				cachedLog.LogLevel == log.LogLevel &&
				cachedLog.Message == log.Message &&
				cachedLog.StackTrace == log.StackTrace &&
				cachedLog.CreatedTime > (log.CreatedTime - TimeSpan.FromSeconds(MaxSecondsToDiscardIdenticalLogMessages));
		}
	}
}
