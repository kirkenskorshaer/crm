using DataLayer;
using DataLayer.MongoData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Administration
{
	public static class Log
	{
		public static Config.LogLevelEnum LogLevel = Config.LogLevelEnum.HeartError | Config.LogLevelEnum.OptionError;
		private static List<DataLayer.MongoData.Log> _logCache = new List<DataLayer.MongoData.Log>();
		public static int MaxLookbackCacheSize = 20;
		public static int MaxSecondsToDiscardIdenticalLogMessages = 60 * 5;

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
			if (LogLevel.HasFlag(logLevel))
			{
				DataLayer.MongoData.Log log = new DataLayer.MongoData.Log()
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

		private static bool IsRepeatedMessage(DataLayer.MongoData.Log log)
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

		private static bool IsSameAndNew(DataLayer.MongoData.Log cachedLog, DataLayer.MongoData.Log log)
		{
			return
				cachedLog.LogLevel == log.LogLevel &&
				cachedLog.Message == log.Message &&
				cachedLog.StackTrace == log.StackTrace &&
				cachedLog.CreatedTime > (log.CreatedTime - TimeSpan.FromSeconds(MaxSecondsToDiscardIdenticalLogMessages));
		}
	}
}
