using System;
using System.Collections.Generic;
using System.Text;
using SystemInterface.Twilio;

namespace TestUtilities
{
	public class TwilioConnectionTester : ITwilioConnection
	{
		public Queue<MessageInfo> SendQueue = new Queue<MessageInfo>();
		private StringBuilder _sendBuilder = new StringBuilder();

		public MessageInfo Send(string toNumber, string messageBody)
		{
			string infoString = $"{toNumber}: {messageBody}";

			if (SendQueue.Count == 0)
			{
				throw new Exception(infoString);
			}

			_sendBuilder.AppendLine(infoString);

			MessageInfo messageInfo = SendQueue.Dequeue();

			return messageInfo;
		}

		public void EnqueueSendReply(MessageInfo reply)
		{
			SendQueue.Enqueue(reply);
		}

		public override string ToString()
		{
			return _sendBuilder.ToString();
		}
	}
}
