using NUnit.Framework;
using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using SystemInterface.Mailrelay.Logic;

namespace SystemInterfaceTest.MailrelayTest.LogicTest
{
	[TestFixture]
	public class SubscriberTest : TestBase
	{
		[Test]
		public void UpdateIfNeededDoesNotUpdateIdenticalSubscribers()
		{
			int id = 1;
			string fullname = "fullname";
			string email = "email";
			Dictionary<string, string> customFields = new Dictionary<string, string>();
			int? groupInCrm = null;

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = new List<getSubscribersReply>()
			{
				new getSubscribersReply()
				{
					email = email,
					name = fullname,
					fields = customFields,
					groups = new List<string>(),
				},
			}
			});

			Subscriber subscriber = new Subscriber(_mailrelayConnectionTester);

			subscriber.UpdateIfNeeded(id, fullname, email, customFields, groupInCrm);

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}

		[Test]
		public void UpdateIfNeededDoesNotRemoveGroups()
		{
			int id = 1;
			string fullname = "fullname";
			string email = "email";
			Dictionary<string, string> customFields = new Dictionary<string, string>();
			int? groupInCrm = null;

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = new List<getSubscribersReply>()
			{
				new getSubscribersReply()
				{
					email = email,
					name = fullname,
					fields = customFields,
					groups = new List<string>() { "1", "2" },
				},
			}
			});

			Subscriber subscriber = new Subscriber(_mailrelayConnectionTester);

			subscriber.UpdateIfNeeded(id, fullname, email, customFields, groupInCrm);

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}

		[Test]
		public void UpdateIfNeededCanUpdateSubscriber()
		{
			int id = 1;
			string fullname = "fullname";
			string email = "email";
			Dictionary<string, string> customFields = new Dictionary<string, string>();
			int? groupInCrm = null;
			List<string> groups = new List<string>() { "1", "2" };

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = new List<getSubscribersReply>()
			{
				new getSubscribersReply()
				{
					email = email,
					name = "not the same fullname",
					fields = customFields,
					groups = groups,
				},
			}
			});

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply()
			{
				status = 1,
				data = 1,
			});

			Subscriber subscriber = new Subscriber(_mailrelayConnectionTester);

			subscriber.UpdateIfNeeded(id, fullname, email, customFields, groupInCrm);

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}
	}
}
