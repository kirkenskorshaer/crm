using System;
using System.Collections.Generic;
using System.Text;
using SystemInterface.Inmobile;
using Utilities.Converter;

namespace TestUtilities
{
	public class InMobileConnectionTester : AbstractInMobileConnection
	{
		public InMobileConnectionTester(string apiKey, string getMessagesGetUrl, string messageStatusCallbackUrl, string postUrl, string hostRootUrl) : base(apiKey, getMessagesGetUrl, messageStatusCallbackUrl, postUrl, hostRootUrl)
		{
		}

		private List<string> _messageIds = new List<string>();
		private Random _random = new Random();
		private List<KeyValuePair<string, InMobileSms>> _messages = new List<KeyValuePair<string, InMobileSms>>();

		public override void GetMessageStatus(Action<string, int, string> receiveStatus)
		{
			int statusCount = _random.Next(_messageIds.Count / 4, _messageIds.Count / 2);

			for (int statusIndex = 0; statusIndex <= statusCount; statusIndex++)
			{
				string MessageId = _messageIds[statusIndex];
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
				_messageIds.Add(sms.Value.MessageId);

				_messages.Add(sms);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach (KeyValuePair<string, InMobileSms> sms in _messages)
			{
				stringBuilder.AppendLine($"{sms.Key}: {sms.Value}");
			}

			return stringBuilder.ToString();
		}
	}
}
