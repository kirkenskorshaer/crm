using Sms.ApiClient.V2;
using Sms.ApiClient.V2.GetMessageStatuses;
using Sms.ApiClient.V2.SendMessages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Inmobile
{
	public class InMobileConnection : AbstractInMobileConnection
	{
		public InMobileConnection(string apiKey, string getMessagesGetUrl, string messageStatusCallbackUrl, string postUrl) : base(apiKey, getMessagesGetUrl, messageStatusCallbackUrl, postUrl)
		{
		}

		public override void Send(Dictionary<string, InMobileSms> inMobileSmsMessages)
		{
			List<ISmsMessage> messages = inMobileSmsMessages.Select(message => message.Value.ToInmobileSms(message.Key)).ToList();

			SendMessagesRequestBuilder requestBuilder = new SendMessagesRequestBuilder();
			SendMessagesClient client = new SendMessagesClient(ApiKey, PostUrl, requestBuilder);

			SendMessagesResponse response = client.SendMessages(messages, MessageStatusCallbackUrl);

			List<MsisdnAndMessageId> responseIds = response.MessageIds;

			responseIds.ForEach(msisdnAndId => inMobileSmsMessages[msisdnAndId.Msisdn].MessageId = msisdnAndId.MessageId);
		}

		public override void GetMessageStatus(Action<string, int, string> receiveStatus)
		{
			GetMessageStatusClient client = new GetMessageStatusClient(ApiKey, GetMessagesGetUrl);
			GetMessageStatusesResponse response = client.GetMessageStatus();

			foreach (MessageStatus status in response.MessageStatuses)
			{
				receiveStatus(status.MessageId, status.StatusCode, status.StatusDescription);
			}
		}

		public override string Version
		{
			get
			{
				return ClientUtils.VersionNumber;
			}
		}
	}
}
