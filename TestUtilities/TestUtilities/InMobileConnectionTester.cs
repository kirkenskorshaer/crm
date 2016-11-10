using System;
using System.Collections.Generic;
using SystemInterface.Inmobile;
using Utilities.Converter;

namespace TestUtilities
{
	public class InMobileConnectionTester : AbstractInMobileConnection
	{
		public InMobileConnectionTester(string apiKey, string getMessagesGetUrl, string messageStatusCallbackUrl, string postUrl) : base(apiKey, getMessagesGetUrl, messageStatusCallbackUrl, postUrl)
		{
		}

		private List<string> _messages = new List<string>();
		private Random _random = new Random();

		public override void GetMessageStatus(Action<string, int, string> receiveStatus)
		{
			int statusCount = _random.Next(_messages.Count / 4, _messages.Count / 2);

			for (int statusIndex = 0; statusIndex <= statusCount; statusIndex++)
			{
				string MessageId = _messages[statusIndex];
				int StatusCode = statusIndex;
				string StatusDescription = statusIndex.ToString();

				receiveStatus(MessageId, StatusCode, StatusDescription);
			}
		}

		public override void Send(Dictionary<string, InMobileSms> inMobileSmsMessages)
		{
			foreach (KeyValuePair<string, InMobileSms> sms in inMobileSmsMessages)
			{
				sms.Value.MessageId = GuidConverter.Convert(_random.Next(), _random.Next(), _random.Next(), _random.Next()).ToString();
				_messages.Add(sms.Value.MessageId);
			}
		}
	}
}
