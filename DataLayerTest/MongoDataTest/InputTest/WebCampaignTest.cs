using DataLayer.MongoData.Input;
using NUnit.Framework;
using System;

namespace DataLayerTest.MongoDataTest.InputTest
{
	[TestFixture]
	public class WebCampaignTest : TestBase
	{
		[Test]
		public void WebCampaignCanBeInserted()
		{
			WebCampaign webCampaign = InsertWebCampaign();

			WebCampaign webCampaignRead = WebCampaign.ReadSingleOrDefault(_mongoConnection, webCampaign.FormId);

			Assert.AreEqual(webCampaign.RedirectTarget, webCampaignRead.RedirectTarget);
		}

		[Test]
		public void StubCanBeAssignedAWebCampaign()
		{
			WebCampaign webcampaign = InsertWebCampaign();

			Stub stub = new Stub();
			stub.WebCampaignId = webcampaign._id;
			stub.Push(_mongoConnection);

			Stub stubRead = Stub.ReadFirst(_mongoConnection);

			Assert.AreEqual(webcampaign._id, stubRead.WebCampaignId);
		}

		[Test]
		public void CreateDefaultWebCampaign()
		{
			WebCampaign webCampaign = new WebCampaign()
			{
				FormId = Guid.Parse("f585cf49-d04f-8a47-ae52-30f59527a9d5"),
				RedirectTarget = "Test.html",
			};

			webCampaign.Insert(_mongoConnection);
		}

		private WebCampaign InsertWebCampaign()
		{
			WebCampaign webCampaign = new WebCampaign()
			{
				FormId = Guid.NewGuid(),
				RedirectTarget = $"url : {Guid.NewGuid()}",
			};

			webCampaign.Insert(_mongoConnection);

			return webCampaign;
		}
	}
}
