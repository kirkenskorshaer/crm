using DataLayer;
using DataLayer.MongoData;
using System.Collections;

namespace Administration
{
	public static class Log
	{
		public static Config.LogLevelEnum LogLevel = Config.LogLevelEnum.HeartError | Config.LogLevelEnum.OptionError;
		private static Queue _logCache = new Queue();
		public static int MaxLookbackCacheSize = 20;

		public static void Write(MongoConnection connection, string message, Config.LogLevelEnum logLevel)
		{
			Write(connection, message, string.Empty, logLevel);
		}

		public static void Write(MongoConnection connection, string message, string stackTrace, Config.LogLevelEnum logLevel)
		{
			if (LogLevel.HasFlag(logLevel))
			{
				if (IsRepeatedError(message, stackTrace) == false)
				{
					DataLayer.MongoData.Log.Write(connection, message, stackTrace, logLevel);
				}
			}
		}

		private static bool IsRepeatedError(string message, string stackTrace)
		{
			string messageAndStackTrace = message;
			if(string.IsNullOrWhiteSpace(stackTrace) == false)
			{
				messageAndStackTrace += stackTrace;
			}

			if(_logCache.Contains(messageAndStackTrace))
			{
				return true;
			}

			if (_logCache.Count >= MaxLookbackCacheSize)
			{
				_logCache.Dequeue();
			}

			_logCache.Enqueue(messageAndStackTrace);

			return false;
        }
	}
}
