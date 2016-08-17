using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseAddMailrelaySubscriberFromLead = DataLayer.MongoData.Option.Options.Logic.AddMailrelaySubscriberFromLead;
using System.Linq;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class AddMailrelaySubscriberFromLeadTest : TestBase
	{
		Lead _lead;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_lead = new Lead(DynamicsCrmConnection);

			_lead.InsertWithoutRead();
		}

		[TearDown]
		new public void TearDown()
		{
			base.TearDown();

			_lead.Delete();
		}

		[Test]
		public void AddMailrelaySubscriberFromLeadTestAddsASubscriber()
		{
			DatabaseWebCampaign webCampaign = GetWebcampaign();

			DatabaseAddMailrelaySubscriberFromLead databaseAddMailrelaySubscriberFromLead = AddMailrelaySubscriberFromLead.CreateIfValid(Connection, Guid.Empty, "test", "test", "test", webCampaign);
			AddMailrelaySubscriberFromLead addMailrelaySubscriberFromLead = new AddMailrelaySubscriberFromLead(Connection, databaseAddMailrelaySubscriberFromLead);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>() { status = 1, data = new List<getSubscribersReply>() });
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = 1 });

			ExecuteWithFakeMailrelayConnection(addMailrelaySubscriberFromLead);

			Console.Out.WriteLine(_mailrelayConnectionTester);

			Assert.IsTrue(_mailrelayConnectionTester.sendFunctions.Any(function => function is addSubscriber));
		}

		[Test]
		public void AddMailrelaySubscriberFromLeadTestCanUpdateASubscriber()
		{
			DatabaseWebCampaign webCampaign = GetWebcampaign();

			string email = "test";
			DatabaseAddMailrelaySubscriberFromLead databaseAddMailrelaySubscriberFromLead = AddMailrelaySubscriberFromLead.CreateIfValid(Connection, _lead.Id, "test", "test", email, webCampaign);
			AddMailrelaySubscriberFromLead addMailrelaySubscriberFromLead = new AddMailrelaySubscriberFromLead(Connection, databaseAddMailrelaySubscriberFromLead);

			int randomId = new Random().Next(0, int.MaxValue);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = new List<getSubscribersReply>()
				{
					new getSubscribersReply()
					{
						email = email,
						id = randomId.ToString(),
						fields = new Dictionary<string, string>(),
						groups = new List<string>(),
					},
				}
			});
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = 1 });

			ExecuteWithFakeMailrelayConnection(addMailrelaySubscriberFromLead);

			Console.Out.WriteLine(_mailrelayConnectionTester);

			updateSubscriber updateSubscriberFunction = (updateSubscriber)_mailrelayConnectionTester.sendFunctions.Single(function => function is updateSubscriber);

			Assert.AreEqual(email, updateSubscriberFunction.email);
			Assert.AreEqual(randomId, updateSubscriberFunction.id);
		}

		private DatabaseWebCampaign GetWebcampaign()
		{
			DatabaseWebCampaign campaign = new DatabaseWebCampaign()
			{
				FormId = Guid.Parse("ff052597-5538-e611-80ef-001c4215c4a0"),
			};

			campaign.Insert(Connection);

			return campaign;
		}
	}
}
