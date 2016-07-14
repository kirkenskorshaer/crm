using System;
using System.Web;
using DatabaseStub = DataLayer.MongoData.Input.Stub;
using DatabaseStubPusher = DataLayer.MongoData.Input.StubPusher;
using DatabaseStubElement = DataLayer.MongoData.Input.StubElement;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using System.Configuration;
using DataLayer;
using System.Linq;

public partial class Stub : System.Web.UI.Page
{
	private string _defaultRedirect = "http://kirkenskorshaer.dk";

	protected void Page_Load(object sender, EventArgs e)
	{
		string errorRedirect = "http://kirkenskorshaer.dk";

		if (Request.HttpMethod != "POST")
		{
			Response.Redirect(errorRedirect);
		}

		string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
		MongoConnection mongoConnection = MongoConnection.GetConnection(databaseName);

		Guid formId = Guid.Empty;
		Guid.TryParse(Request.Form["formId"], out formId);

		DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadSingleOrDefault(mongoConnection, formId);

		DatabaseStub stub = CreateStub(webCampaign);

		CollectFields(stub);

		AddOprindelseIp(stub);

		AddOprindelse(stub);

		DatabaseStubPusher.GetInstance(mongoConnection).Push(stub);

		Redirect(webCampaign);
	}

	private static DatabaseStub CreateStub(DatabaseWebCampaign webCampaign)
	{
		DatabaseStub stub = new DatabaseStub()
		{
			PostTime = DateTime.Now,
		};

		if (webCampaign != null)
		{
			stub.WebCampaignId = webCampaign._id;
		}

		return stub;
	}

	private void CollectFields(DatabaseStub stub)
	{
		foreach (string key in Request.Form)
		{
			DatabaseStubElement element = new DatabaseStubElement()
			{
				Key = HttpUtility.HtmlDecode(key),
				Value = HttpUtility.HtmlDecode(Request.Form[key]),
			};

			stub.Contents.Add(element);
		}
	}

	private void AddOprindelseIp(DatabaseStub stub)
	{
		if (stub.Contents.Any(lElement => lElement.Key == "new_oprindelseip"))
		{
			return;
		}

		DatabaseStubElement element = new DatabaseStubElement()
		{
			Key = "new_oprindelseip",
			Value = Request.UserHostAddress,
		};

		stub.Contents.Add(element);
	}

	private void AddOprindelse(DatabaseStub stub)
	{
		if (Request.UrlReferrer == null || stub.Contents.Any(lElement => lElement.Key == "new_oprindelse"))
		{
			return;
		}

		DatabaseStubElement element = new DatabaseStubElement()
		{
			Key = "new_oprindelse",
			Value = Request.UrlReferrer.AbsoluteUri,
		};

		stub.Contents.Add(element);
	}

	private void Redirect(DatabaseWebCampaign webCampaign)
	{
		string redirectTarget = _defaultRedirect;
		if (webCampaign != null)
		{
			redirectTarget = webCampaign.RedirectTarget;
		};

		Response.Redirect(redirectTarget);
	}
}