﻿using System.Net;

namespace SystemInterface
{
	public class Email
	{
		public void Send(string messageBody, bool isBodyHtml, string messageSubject, string messageFrom, string messageTo, string smtpHost, int smtpPort, string userName, string password)
		{
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
			message.To.Add(messageTo);
			message.Subject = messageSubject;
			message.From = new System.Net.Mail.MailAddress(messageFrom);
			message.Body = messageBody;
			message.IsBodyHtml = isBodyHtml;

			System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smtpHost, smtpPort);
			smtp.Credentials = new NetworkCredential(userName, password);
			smtp.Send(message);
		}
	}
}
