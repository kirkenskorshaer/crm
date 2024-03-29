﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using SystemInterface.Mailrelay;
using SystemInterface.Mailrelay.FunctionReply;

namespace TestUtilities
{
	public class MailrelayConnectionTester : IMailrelayConnection
	{
		public Queue<AbstractMailrelayReply> replies = new Queue<AbstractMailrelayReply>();
		public List<AbstractFunction> sendFunctions = new List<AbstractFunction>();

		public AbstractMailrelayReply Send(AbstractFunction functionToSend)
		{
			if (replies.Count == 0)
			{
				throw new Exception($"reply queue was empty when sending {HttpUtility.UrlDecode(functionToSend.ToGet())}");
			}

			sendFunctions.Add(functionToSend);

			AbstractMailrelayReply reply = replies.Dequeue();

			return reply;
		}

		public override string ToString()
		{
			StringBuilder raportBuilder = new StringBuilder();

			foreach (AbstractFunction function in sendFunctions)
			{
				raportBuilder.AppendLine(HttpUtility.UrlDecode(function.ToGet()));
			}

			return raportBuilder.ToString();
		}
	}
}
