using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay
{
	public class MailrelayConnection : IMailrelayConnection
	{
		private string mailrelayUrl;
		private string mailrelayApiKey;

		public MailrelayConnection(string url, string apiKey)
		{
			mailrelayUrl = url;
			mailrelayApiKey = apiKey;
		}

		public AbstractMailrelayReply Send(AbstractFunction functionToSend)
		{
			functionToSend.apiKey = mailrelayApiKey;
			string target = mailrelayUrl + functionToSend.ToGet();

			Dictionary<string, string> values = functionToSend.GetValues();

			string reply;

			using (HttpClient client = new HttpClient())
			{
				FormUrlEncodedContent content = new FormUrlEncodedContent(values);

				try
				{
					Task<HttpResponseMessage> response = client.PostAsync(mailrelayUrl, content);
					Task<string> responseString = response.Result.Content.ReadAsStringAsync();
					reply = responseString.Result;
				}
				catch (Exception exception)
				{
					throw new MailrelayConnectionException(functionToSend, mailrelayUrl, exception);
				}
			}

			AbstractMailrelayReply mailrelayReply = functionToSend.GetMailrelayReply(reply);

			if (mailrelayReply.status == 0)
			{
				throw new MailrelayConnectionException(functionToSend, mailrelayReply, mailrelayUrl);
			}

			return mailrelayReply;
		}

		public AbstractMailrelayReply SendAsGet(AbstractFunction functionToSend)
		{
			functionToSend.apiKey = mailrelayApiKey;
			string target = mailrelayUrl + functionToSend.ToGet();

			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(target);
			httpWebRequest.ContentType = "text/xml; encoding='utf-8'";
			httpWebRequest.Method = "GET";

			HttpWebResponse httpResponse;
			try
			{
				httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			}
			catch (Exception exception)
			{
				throw new MailrelayConnectionException(functionToSend, mailrelayUrl, exception);
			}

			string reply;
			using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				reply = streamReader.ReadToEnd();
			}

			AbstractMailrelayReply mailrelayReply = functionToSend.GetMailrelayReply(reply);

			if (mailrelayReply.status == 0)
			{
				throw new MailrelayConnectionException(functionToSend, mailrelayReply, mailrelayUrl);
			}

			return mailrelayReply;
		}
	}
}
