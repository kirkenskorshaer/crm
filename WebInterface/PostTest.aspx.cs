using DataLayer;
using DataLayer.MongoData;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;

public partial class PostTest : System.Web.UI.Page
{
	private string _logfilePath;
	private string _logfileName = "postlog.txt";
	private string _pathAndName;

	protected void Page_Load(object sender, EventArgs e)
	{
		string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
		MongoConnection mongoConnection = MongoConnection.GetConnection(databaseName);

		Config config = Config.GetConfig(mongoConnection);

		if (config.EnableTest != true)
		{
			return;
		}

		_logfilePath = config.GetResourcePath("test");
		_pathAndName = _logfilePath + "/" + _logfileName;

		if (Request.HttpMethod != "POST")
		{
			if (Request.Params["clear"] == "true")
			{
				ClearFile();
				Response.Redirect("PostTest.aspx");
				return;
			}

			string fileContent = GetFileContent();
			form1.InnerHtml = "<a href=\"PostTest.aspx?clear=true\">Clear</a>" + fileContent;
			return;
		}

		LogRequest();

		WriteResponse();
	}

	private void WriteResponse()
	{
		string function = Request.Form["function"];

		if (string.IsNullOrWhiteSpace(function))
		{
			return;
		}

		string response = GetResponseByFunction(function);

		if (string.IsNullOrWhiteSpace(response))
		{
			return;
		}

		Response.Clear();
		Response.ContentType = "application/json; charset=utf-8";
		Response.Write(response);
		Response.End();
	}

	private string GetResponseByFunction(string function)
	{
		switch (function)
		{
			case "sendMail":
				return "{status:1,error:\"test error\",data:true}";
			default:
				break;
		}

		return string.Empty;
	}

	private void LogRequest()
	{
		StringBuilder logBuilder = new StringBuilder();

		logBuilder.Append("<table><caption>");
		logBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
		logBuilder.Append("</caption>");

		foreach (string key in Request.Form)
		{
			logBuilder.Append("<tr><td>");
			logBuilder.Append(HttpUtility.HtmlDecode(key));
			logBuilder.Append("</td>");
			logBuilder.Append("<td>");
			logBuilder.Append(HttpUtility.HtmlDecode(Request.Form[key]));
			logBuilder.Append("</td></tr>");
		}
		logBuilder.Append("</table></br></br>");
		File.AppendAllText(_pathAndName, logBuilder.ToString());
	}

	private void ClearFile()
	{
		if (MakeSureFileExists() == false)
		{
			File.Create(_pathAndName).Close();
		}
	}

	private string GetFileContent()
	{
		MakeSureFileExists();

		return File.ReadAllText(_pathAndName);
	}

	private bool MakeSureFileExists()
	{
		if (File.Exists(_pathAndName) == false)
		{
			Directory.CreateDirectory(_logfilePath);
			File.Create(_pathAndName).Close();

			return true;
		}

		return false;
	}
}