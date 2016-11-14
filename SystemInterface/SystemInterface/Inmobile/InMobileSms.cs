using Sms.ApiClient.V2.SendMessages;
using System;
using System.Text;

namespace SystemInterface.Inmobile
{
	public class InMobileSms
	{
		public string Text { get; private set; }
		public string SenderName { get; private set; }
		public string MessageId { get; set; }
		public DateTime? SendTime { get; private set; }

		public object LocalSms;

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
				return "45" + trimmedNumber;
			}

			return trimmedNumber;
		}

		internal ISmsMessage ToInmobileSms(string msisdn)
		{
			ISmsMessage message = new SmsMessage(msisdn, Text, SenderName, SmsEncoding.Utf8, MessageId, SendTime, false);

			return message;
		}

		public override string ToString()
		{
			return $"Text:{Text} SenderName:{SenderName} MessageId:{MessageId} SendTime:{SendTime?.ToString("yyyy-MM-dd HH:mm:ss")}";
		}
	}
}
