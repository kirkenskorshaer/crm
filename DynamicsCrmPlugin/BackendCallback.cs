using System;
using Microsoft.Xrm.Sdk;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace DynamicsCrmPlugin
{
	public sealed class BackendCallback : IPlugin
	{
		private string targetUrl;
		private const char replyLineDelimeter = ';';
		private const char replyKeyValueDelimeter = ':';

		public BackendCallback(string config)
		{
			if (string.IsNullOrWhiteSpace(config))
			{
				targetUrl = "http://localhost";
			}
			else
			{
				targetUrl = config;
			}
		}

		public void Execute(IServiceProvider serviceProvider)
		{
			ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

			try
			{
				tracingService.Trace("Target Url: " + targetUrl);

				IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
				Dictionary<string, string> parametersToSend = BuildParametersToSend(context);
				TraceParemeters(tracingService, "Sending:", parametersToSend);

				string reply = Send(parametersToSend, targetUrl);
				Dictionary<string, string> parametersReceived = ReadParametersFromReply(reply);
				TraceParemeters(tracingService, "Receiving:", parametersReceived);

				WriteToOutputParameters(context, parametersReceived);
			}
			catch (Exception exception)
			{
				tracingService.Trace($"Exception: {exception}");
				throw;
			}
		}

		private void WriteToOutputParameters(IPluginExecutionContext context, Dictionary<string, string> parametersReceived)
		{
			foreach (KeyValuePair<string, string> parameter in parametersReceived)
			{
				if (context.OutputParameters.ContainsKey(parameter.Key))
				{
					object value = GetObjectFromString(parameter.Value);

					context.OutputParameters[parameter.Key] = value;
				}
			}
		}

        private object GetObjectFromString(string value)
		{
			byte[] inputBytes = Convert.FromBase64String(value);
			MemoryStream valueStream = new MemoryStream(inputBytes);

			object deserializedObject;

			try
			{
				using (valueStream)
				{
					BinaryFormatter formatter = new BinaryFormatter();
					deserializedObject = formatter.Deserialize(valueStream);
				}
			}
			catch (SerializationException)
			{
				throw;
			}
			finally
			{
				valueStream.Close();
			}

			return deserializedObject;
		}

		private void TraceParemeters(ITracingService tracingService, string headline, Dictionary<string, string> parametersToSend)
		{
			tracingService.Trace(headline);

			parametersToSend.ToList().ForEach(parameter => tracingService.Trace($"{parameter.Key} : {parameter.Value}"));
		}

		private Dictionary<string, string> BuildParametersToSend(IPluginExecutionContext context)
		{
			Dictionary<string, string> parametersToSend = new Dictionary<string, string>();

			foreach (KeyValuePair<string, object> parameter in context.InputParameters)
			{
				if (parameter.Key == "Target")
				{
					continue;
				}

				parametersToSend.Add(parameter.Key, parameter.Value.ToString());
			}

			return parametersToSend;
		}

		private Dictionary<string, string> ReadParametersFromReply(string replyString)
		{
			Dictionary<string, string> parametersReceived = new Dictionary<string, string>();

			string[] replyLines = replyString.Split(replyLineDelimeter);
			foreach (string replyLine in replyLines)
			{
				string[] replyKeyValue = replyLine.Split(replyKeyValueDelimeter);
				if (replyKeyValue.Length == 2)
				{
					string key = replyKeyValue[0];
					string value = replyKeyValue[1];

					if (parametersReceived.ContainsKey(key) == false)
					{
						parametersReceived.Add(key, value);
					}
				}
			}

			return parametersReceived;
		}

		private string Send(Dictionary<string, string> values, string targetUrl)
		{
			string replyString = string.Empty;

			using (HttpClient client = new HttpClient())
			{
				FormUrlEncodedContent content = new FormUrlEncodedContent(values);

				Task<HttpResponseMessage> response = client.PostAsync(targetUrl, content);
				Task<string> responseString = response.Result.Content.ReadAsStringAsync();
				replyString = responseString.Result;
			}

			return replyString;
		}
	}
}
