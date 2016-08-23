using NUnit.Framework;
using System;
using SystemInterface.Mailrelay;
using SystemInterface.Mailrelay.FunctionReply;
using SystemInterface.Mailrelay.Function.Groups;
using SystemInterface.Mailrelay.Function.Statistics;
using SystemInterface.Mailrelay.Function.CustomFields;
using SystemInterface.Mailrelay.Function.Subscribers;
using System.Collections.Generic;
using SystemInterface.Mailrelay.Function.SMTP;
using SystemInterface.Mailrelay.Function.LogFunctions;
using SystemInterface.Mailrelay.Function.Mailboxes;
using SystemInterface.Mailrelay.Function.Campaigns;

namespace SystemInterfaceTest.MailrelayTest
{
	[TestFixture]
	public class MailrelayConnectionTest : TestBase
	{
		[Test]
		public void getStats()
		{
			getStats function = new getStats() { startDate = DateTime.Now.AddDays(1), endDate = DateTime.Now };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		public void getGroups()
		{
			getGroups function = new getGroups() { sortOrder = AbstractFunction.sortOrderEnum.ASC };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		public void MailrelayWillNotBeCalledTooOften()
		{
			getGroups function = new getGroups() { offset = 0, count = 1, sortOrder = AbstractFunction.sortOrderEnum.ASC };

			TimeSpan waitTime = TimeSpan.FromSeconds(15);

			((MailrelayConnection)_mailrelayConnection).sendInterval = waitTime;

			DateTime beforeCall = DateTime.Now;

			_mailrelayConnection.Send(function);
			_mailrelayConnection.Send(function);

			DateTime after2Calls = DateTime.Now;

			TimeSpan timeToMake2Calls = after2Calls - beforeCall;

			Assert.Greater(timeToMake2Calls, waitTime);
		}

		[Test]
		[Ignore]
		public void addGroup()
		{
			addGroup function = new addGroup() { description = "this is a test group", enable = false, name = "test", position = 3, visible = true };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void updateGroup()
		{
			updateGroup function = new updateGroup() { id = 2, description = "this is test group 2", enable = false, name = "test", visible = true, position = 4 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void deleteGroup()
		{
			deleteGroup function = new deleteGroup() { id = 2 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getCustomFields()
		{
			getCustomFields function = new getCustomFields() { };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void addCustomField()
		{
			addCustomField function = new addCustomField() { defaultField = "testDefault", enable = true, fieldType = AbstractCustomField.CustomFieldTypeEnum.TextField, name = "test", position = 1 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void updateCustomField()
		{
			updateCustomField function = new updateCustomField() { id = 1, defaultField = "testDefault", enable = true, fieldType = AbstractCustomField.CustomFieldTypeEnum.TextField, name = "test", position = 1 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void deleteCustomField()
		{
			deleteCustomField function = new deleteCustomField() { id = 2 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void addSubscriber()
		{
			addSubscriber function = new addSubscriber()
			{
				email = "svend.l.kirkenskorshaer@gmail.com",
				groups = new List<int>() { 1 },
				name = "test_name",
				customFields = new Dictionary<string, string>()
				{
					{ "f_1", "f1" },
					{ "f_3", "f3" },
					{ "f_4", "f4" },
					{ "f_5", "f5" },
				},
			};

			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		public void getSubscribers()
		{
			getSubscribers function = new getSubscribers() { };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void updateSubscribers()
		{
			updateSubscribers function = new updateSubscribers() { ids = new List<int>() { 2 }, activated = true, banned = false, deleted = false };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void updateSubscriber()
		{
			updateSubscriber function = new updateSubscriber()
			{
				id = 4,
				email = "svend.l.kirkenskorshaer@gmail.com_test",
				name = "test_name",
				activated = true,
				groups = new List<int>() { 1 },
				customFields = new Dictionary<string, string>()
				{
					{ "f_1", "f1" },
					{ "f_3", "f3" },
					{ "f_4", "f4" },
					{ "f_5", "f5" },
				},
			};
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void assignSubscribersToGroups()
		{
			assignSubscribersToGroups function = new assignSubscribersToGroups() { groups = new List<int>() { 3 }, subscribers = new List<string>() { "svend.l.kirkenskorshaer@gmail.com_test2" } };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void unsubscribe()
		{
			unsubscribe function = new unsubscribe() { email = "svend.l.kirkenskorshaer@gmail.com" };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void deleteSubscriber()
		{
			deleteSubscriber function = new deleteSubscriber() { email = "svend.l.kirkenskorshaer@gmail.com" };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getSmtpTags()
		{
			getSmtpTags function = new getSmtpTags() { count = 10, offset = 0 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getSends()
		{
			getSends function = new getSends() { count = 10 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getDeliveryErrors()
		{
			getDeliveryErrors function = new getDeliveryErrors() { count = 10 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getDayLog()
		{
			getDayLog function = new getDayLog() { count = 10 };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getMailRcptNumber()
		{
			getMailRcptNumber function = new getMailRcptNumber() { email = "svend.l.kirkenskorshaer@gmail.com", date = DateTime.Now };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getMailRcptInfo()
		{
			getMailRcptInfo function = new getMailRcptInfo() { id = 1, email = "svend.l.kirkenskorshaer@gmail.com", date = DateTime.Now };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getPackages()
		{
			getPackages function = new getPackages() { sortOrder = AbstractFunction.sortOrderEnum.DESC };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void sendMail()
		{
			sendMail function = new sendMail()
			{
				emails = new Dictionary<string, string>()
				{
					{ "Svend", "svend.l.kirkenskorshaer@gmail.com" }
				},
				subject = "test",
				html = "hej [name] vi tester igen",
				mailboxFromId = 1,
				mailboxReplyId = 1,
				mailboxReportId = 1,
				packageId = 6,
			};
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getMailboxes()
		{
			getMailboxes function = new getMailboxes() { };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		[Test]
		[Ignore]
		public void getCampaigns()
		{
			getCampaigns function = new getCampaigns() { };
			AbstractMailrelayReply reply = TestCall(function);
			Assert.AreEqual(1, reply.status);
		}

		private AbstractMailrelayReply TestCall(AbstractFunction functionToCall)
		{
			AbstractMailrelayReply reply = _mailrelayConnection.Send(functionToCall);

			Console.Out.WriteLine(functionToCall.ToGet());
			Console.Out.WriteLine(reply.ToJson());

			return reply;
		}
	}
}
