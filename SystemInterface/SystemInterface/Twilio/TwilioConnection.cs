using Twilio;

namespace SystemInterface.Twilio
{
	public class TwilioConnection : ITwilioConnection
	{
		private string _accountSid;
		private string _authToken;
		private string _statusCallback;
		private string _fromNumber;
		private TwilioRestClient _client;

		public TwilioConnection(string fromNumber, string accountSid, string authToken, string statusCallback)
		{
			_accountSid = accountSid;
			_authToken = authToken;
			_fromNumber = fromNumber;
			_statusCallback = statusCallback;

			_client = new TwilioRestClient(accountSid, authToken);
		}

		public MessageInfo Send(string toNumber, string messageBody)
		{
			Message message = _client.SendMessage(_fromNumber, toNumber, messageBody, _statusCallback);

			MessageInfo messageInfo = new MessageInfo(message);

			return messageInfo;
		}
	}
}
