using System;
using Twilio;

namespace SystemInterface.Twilio
{
	public class MessageInfo
	{
		public string Body { get; private set; }
		public DateTime DateCreated { get; private set; }
		public DateTime DateSent { get; private set; }
		public DateTime DateUpdated { get; private set; }
		public int? ErrorCode { get; private set; }
		public string ErrorMessage { get; private set; }
		public string From { get; private set; }
		public int NumImages { get; private set; }
		public decimal Price { get; private set; }
		public string To { get; private set; }
		public string Sid { get; private set; }
		public StatusEnum Status { get; private set; }

		private RestException _restException;

		public enum StatusEnum
		{
			queued = 1,
			sending = 2,
			sent = 3,
			failed = 4,
		}

		public MessageInfo()
		{
		}

		public MessageInfo(Message message)
		{
			Body = message.Body;
			DateCreated = message.DateCreated;
			DateSent = message.DateSent;
			DateUpdated = message.DateUpdated;
			ErrorCode = message.ErrorCode;
			ErrorMessage = message.ErrorMessage;
			From = message.From;
			NumImages = message.NumImages;
			Price = message.Price;
			To = message.To;
			Sid = message.Sid;
			Status = GetStatus(message.Status);

			_restException = message.RestException;
		}

		private StatusEnum GetStatus(string status)
		{
			switch (status.ToLower())
			{
				case "failed":
					return StatusEnum.failed;
				case "queued":
					return StatusEnum.queued;
				case "sending":
					return StatusEnum.sending;
				case "sent":
					return StatusEnum.sent;
				default:
					break;
			}

			throw new ArgumentException($"unknown status {status}");
		}

		public void VerifyRestException()
		{
			if (_restException == null)
			{
				return;
			}

			throw new Exception($"Code: {_restException.Code} Message: {_restException.Message} MoreInfo: {_restException.MoreInfo} Status: {_restException.Status}");
		}
	}
}
