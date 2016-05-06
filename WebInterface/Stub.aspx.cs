using System;
using System.Web;
using DatabaseStub = DataLayer.MongoData.Input.Stub;
using DatabaseStubPusher = DataLayer.MongoData.Input.StubPusher;
using DatabaseStubElement = DataLayer.MongoData.Input.StubElement;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using System.Configuration;
using DataLayer;

public partial class Stub : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		string errorRedirect = "error.html";

		if (Request.HttpMethod != "POST")
		{
			Response.Redirect(errorRedirect);
		}

		string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
		MongoConnection mongoConnection = MongoConnection.GetConnection(databaseName);

		Guid formId = Guid.Empty;
		Guid.TryParse(Request.Form["formId"], out formId);

		DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadSingleOrDefault(mongoConnection, formId);

		if (webCampaign == null)
		{
			Response.Redirect(errorRedirect);
		}

		DatabaseStub stub = new DatabaseStub()
		{
			PostTime = DateTime.Now,
			WebCampaignId = webCampaign._id,
		};

		foreach (string key in Request.Form)
		{
			if(key == "formId")
			{
				continue;
			}

			DatabaseStubElement element = new DatabaseStubElement()
			{
				Key = key,
				//b = Request.Form[key],
				Value = HttpUtility.HtmlDecode(Request.Form[key]),
			};

			stub.Contents.Add(element);
		}

		DatabaseStubPusher.GetInstance(mongoConnection).Push(stub);

		string redirectTarget = "error.html";
		if (webCampaign != null)
		{
			redirectTarget = webCampaign.RedirectTarget;
		};

		Response.Redirect(redirectTarget);
	}
}