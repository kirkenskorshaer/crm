using System;
using System.Web;
using System.Collections.Specialized;
using System.Collections.Generic;
using Utilities.Converter;
using System.Text;
using System.Configuration;
using DataLayer;
using DataLayer.MongoData.Option.Options.Logic;
using WebAdministration;

public partial class Backend : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Dictionary<string, object> inputParameters = ReadInputParameters();

		string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
		MongoConnection mongoConnection = MongoConnection.GetConnection(databaseName);

		BackendMessage message = BackendMessage.Create(mongoConnection, "Backend Message", inputParameters);

		OptionHandler handler = new OptionHandler(mongoConnection);
		Dictionary<string, object> outputParameters = handler.GetResponse(message);

		SendOutputParameters(outputParameters);
	}

	private void SendOutputParameters(Dictionary<string, object> outputParameters)
	{
		Response.Clear();

		StringBuilder responseBuilder = new StringBuilder();

		bool isFirst = true;
		foreach (KeyValuePair<string, object> parameter in outputParameters)
		{
			string serializedValue = StringConverter.SerializeToString(parameter.Value);

			if (isFirst == false)
			{
				responseBuilder.Append(";");
			}

			responseBuilder.Append(parameter.Key);
			responseBuilder.Append(":");
			responseBuilder.Append(serializedValue);

			isFirst = false;
		}

		Response.Write(responseBuilder.ToString());
	}

	private Dictionary<string, object> ReadInputParameters()
	{
		NameValueCollection input = Request.Form;

		if (Request.HttpMethod == "POST")
		{
			input = Request.Form;
		}
		else
		{
			input = Request.QueryString;
		}

		Dictionary<string, object> inputParameters = new Dictionary<string, object>();

		foreach (string key in input)
		{
			string keyString = HttpUtility.HtmlDecode(key);
			string valueString = HttpUtility.HtmlDecode(input[key]);

			object valueObject = StringConverter.DeserializeFromString(valueString);

			if (inputParameters.ContainsKey(keyString) == false)
			{
				inputParameters.Add(keyString, valueObject);
			}
		}

		return inputParameters;
	}
}