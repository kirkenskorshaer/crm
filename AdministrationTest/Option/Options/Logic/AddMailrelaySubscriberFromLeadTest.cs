using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseAddMailrelaySubscriberFromLead = DataLayer.MongoData.Option.Options.Logic.AddMailrelaySubscriberFromLead;
using System.Linq;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using System.Collections.Generic;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class AddMailrelaySubscriberFromLeadTest : TestBase
	{
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

			DatabaseAddMailrelaySubscriberFromLead databaseAddMailrelaySubscriberFromLead = AddMailrelaySubscriberFromLead.CreateIfValid(Connection, Guid.Empty, "test", "test", "test", webCampaign);
			AddMailrelaySubscriberFromLead addMailrelaySubscriberFromLead = new AddMailrelaySubscriberFromLead(Connection, databaseAddMailrelaySubscriberFromLead);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = new List<getSubscribersReply>()
				{
					new getSubscribersReply()
					{
						fields = new Dictionary<string, string>(),
						groups = new List<string>(),
					},
				}
			});
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = 1 });

			ExecuteWithFakeMailrelayConnection(addMailrelaySubscriberFromLead);

			Console.Out.WriteLine(_mailrelayConnectionTester);

			Assert.IsTrue(_mailrelayConnectionTester.sendFunctions.Any(function => function is updateSubscriber));
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
