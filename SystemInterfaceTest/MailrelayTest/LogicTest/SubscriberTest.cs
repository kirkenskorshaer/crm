using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

		[Test]
		public void SubscribersEnumerationReturnSubscribers()
		{
			Dictionary<string, string> customFields = new Dictionary<string, string>();
			List<string> groups12 = new List<string>() { "1", "2" };

			EnqueueSubscribersReply(customFields, groups12, 2);
			EnqueueSubscribersReply(customFields, groups12, 2);
			EnqueueSubscribersReply(customFields, groups12, 2);
			EnqueueSubscribersReply(customFields, groups12, 1);

			Subscriber subscriber = new Subscriber(_mailrelayConnectionTester);

			getSubscribers getSubscribersFunction = new getSubscribers();

			IEnumerable<getSubscribersReply> replies = subscriber.GetMailrelaySubscribers(getSubscribersFunction, 2);

			Assert.AreEqual(0, _mailrelayConnectionTester.sendFunctions.Count);

			replies.ToList();

			Assert.AreEqual(4, _mailrelayConnectionTester.sendFunctions.Count);

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}

		private void EnqueueSubscribersReply(Dictionary<string, string> customFields, List<string> groups, int replies)
		{
			List<getSubscribersReply> data = new List<getSubscribersReply>();

			for (int replyIndex = 0; replyIndex < replies; replyIndex++)
			{
				data.Add(new getSubscribersReply() { email = "email", name = "name", fields = customFields, groups = groups, });
			}

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = data,
			});
		}
	}
}
