using Sms.ApiClient.V2.SendMessages;
using System;

namespace SystemInterface.Inmobile
{
	public class InMobileSms
	{
		public string Text { get; private set; }
		public string SenderName { get; private set; }
		public string MessageId { get; set; }
		public DateTime? SendTime { get; private set; }

		public InMobileSms(string text, string senderName, DateTime? sendTime)
		{
			Text = text;
			SenderName = senderName;
			SendTime = sendTime;
		}

		public static string GetMsisdn(string crmNumber)
		{
			string trimmedNumber = crmNumber.Replace(" ", "");

			if (trimmedNumber.Length == 8)
			{
				return "45" + crmNumber;
			}

			return crmNumber;
		}

		internal ISmsMessage ToInmobileSms(string msisdn)
		{
			ISmsMessage message = new SmsMessage(msisdn, Text, SenderName, SmsEncoding.Utf8, MessageId, SendTime, false);

			return message;
		}
	}
}
