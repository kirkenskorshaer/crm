using System;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay
{
	public class MailrelayConnectionException : Exception
	{
		public MailrelayConnectionException(AbstractFunction senderFunction, AbstractMailrelayReply reply, string targetUrl)
			: base(GetMessage(senderFunction, reply, targetUrl))
		{
		}

		public MailrelayConnectionException(AbstractFunction senderFunction, AbstractMailrelayReply reply, string targetUrl, Exception inner)
			: base($"Error:	{inner.Message}" + GetMessage(senderFunction, reply, targetUrl), inner)
		{
		}

		public MailrelayConnectionException(AbstractFunction senderFunction, string targetUrl)
			: base(GetMessage(senderFunction, targetUrl))
		{
		}

		public MailrelayConnectionException(AbstractFunction senderFunction, string targetUrl, Exception inner)
			: base($"Error:	{inner.Message}" + GetMessage(senderFunction, targetUrl), inner)
		{
		}

		private static string GetMessage(AbstractFunction senderFunction, AbstractMailrelayReply reply, string targetUrl)
		{
			string parameters = senderFunction.ToGet();
			string error = reply.error;

			return $"{senderFunction.GetType().Name}{Environment.NewLine}	Url:{targetUrl}{Environment.NewLine}	Parameters:{parameters}{Environment.NewLine}	Error:{error}";
		}

		private static string GetMessage(AbstractFunction senderFunction, string targetUrl)
		{
			string parameters = senderFunction.ToGet();

			return $"{senderFunction.GetType().Name}{Environment.NewLine}	Url:{targetUrl}{Environment.NewLine}	Parameters:{parameters}{Environment.NewLine}";
		}
	}
}
