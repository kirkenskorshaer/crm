using System;
using System.Collections.Generic;

namespace SystemInterface.Inmobile
{
	public abstract class AbstractInMobileConnection
	{
		public string ApiKey { get; private set; }
		public string GetMessagesGetUrl { get; private set; }
		public string MessageStatusCallbackUrl { get; private set; }
		public string PostUrl { get; private set; }
		public string HostRootUrl { get; private set; }

		public AbstractInMobileConnection(string apiKey, string getMessagesGetUrl, string messageStatusCallbackUrl, string postUrl, string hostRootUrl)
		{
			ApiKey = apiKey;
			GetMessagesGetUrl = getMessagesGetUrl;
			MessageStatusCallbackUrl = messageStatusCallbackUrl;
			PostUrl = postUrl;
			HostRootUrl = hostRootUrl;
		}

		public abstract void Send(Dictionary<string, InMobileSms> inMobileSmsMessages);
		public abstract void GetMessageStatus(Action<string, int, string> receiveStatus);

		public virtual string Version
		{
			get
			{
				return "Not Implemented";
			}
		}
	}
}
