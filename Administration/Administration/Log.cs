using DataLayer;
using DataLayer.MongoData;

namespace Administration
{
	public static class Log
	{
		public static Config.LogLevelEnum LogLevel = Config.LogLevelEnum.HeartError | Config.LogLevelEnum.OptionError;

		public static void Write(MongoConnection connection, string message, Config.LogLevelEnum logLevel)
		{
			Write(connection, message, string.Empty, logLevel);
		}

		public static void Write(MongoConnection connection, string message, string stackTrace, Config.LogLevelEnum logLevel)
		{
			if (LogLevel.HasFlag(logLevel))
			{
				DataLayer.MongoData.Log.Write(connection, message, stackTrace, logLevel);
			}
		}
	}
}
