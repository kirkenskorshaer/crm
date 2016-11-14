using DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using SystemInterface.Inmobile;

namespace Administration.TestToLog
{
	public class InMobileConnectionTestToLog : AbstractInMobileConnection
	{
		private MongoConnection _mongoConnection;

		public InMobileConnectionTestToLog(MongoConnection mongoConnection, string apiKey, string getMessagesGetUrl, string messageStatusCallbackUrl, string postUrl) : base(apiKey, getMessagesGetUrl, messageStatusCallbackUrl, postUrl)
		{
			_mongoConnection = mongoConnection;
		}

		public override void GetMessageStatus(Action<string, int, string> receiveStatus)
		{
		}

		public override void Send(Dictionary<string, InMobileSms> inMobileSmsMessages)
		{
			StringBuilder logBuilder = new StringBuilder();

			foreach (KeyValuePair<string, InMobileSms> smsByMsisdn in inMobileSmsMessages)
			{
				logBuilder.AppendLine($"{smsByMsisdn.Key}: {smsByMsisdn.Value}");
			}

			Log.WriteLocation(_mongoConnection, logBuilder.ToString(), "InMobileConnectionTestToLog", DataLayer.MongoData.Config.LogLevelEnum.OptionReport);
		}
	}
}
