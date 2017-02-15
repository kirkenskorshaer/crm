using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using DatabaseUpdateMailrelayGroup = DataLayer.MongoData.Option.Options.Logic.UpdateMailrelayGroup;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class UpdateMailrelayGroupTest : TestBase
	{
		private Contact _contact;
		private MarketingList _list;
		private int _groupId;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_contact = CreateContact();
			_contact.new_mailrelaysubscriberid = _random.Next(0, int.MaxValue);
			_contact.InsertWithoutRead();

			_groupId = _random.Next(0, int.MaxValue);

			_list = InsertMarketingListOnCampaign(_contact);
		}

		[Test]
		public void ExistingGroupMemberWillNotBeUpdated()
		{
			DatabaseUpdateMailrelayGroup databaseUpdateMailrelayGroup = CreateDatabaseUpdateMailrelayGroup(_list);

			UpdateMailrelayGroup updateMailrelayGroup = new UpdateMailrelayGroup(Connection, databaseUpdateMailrelayGroup);
			updateMailrelayGroup.ChangeMailrelayConnection(_mailrelayConnectionTester);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = _groupId });
			EnqueueGetSubscribersReply(_contact.new_mailrelaysubscriberid.Value);

			updateMailrelayGroup.ExecuteOption(new Administration.Option.Options.OptionReport(typeof(UpdateMailrelayGroupTest)));

			Assert.AreEqual(0, _mailrelayConnectionTester.sendFunctions.Count(function => function.GetType() == typeof(updateSubscriber)));
		}

		[Test]
		public void NewGroupWillNotBeCreatedIfItExists()
		{
			DatabaseUpdateMailrelayGroup databaseUpdateMailrelayGroup = CreateDatabaseUpdateMailrelayGroup(_list);
			_list.new_mailrelaygroupid = _groupId;
			_list.UpdateMailrelaygroupid(DynamicsCrmConnection);

			UpdateMailrelayGroup updateMailrelayGroup = new UpdateMailrelayGroup(Connection, databaseUpdateMailrelayGroup);
			updateMailrelayGroup.ChangeMailrelayConnection(_mailrelayConnectionTester);

			EnqueueGetSubscribersReply(_contact.new_mailrelaysubscriberid.Value);

			updateMailrelayGroup.ExecuteOption(new Administration.Option.Options.OptionReport(typeof(UpdateMailrelayGroupTest)));
		}

		[Test]
		public void NewGroupMemberWillBeAdded()
		{
			DatabaseUpdateMailrelayGroup databaseUpdateMailrelayGroup = CreateDatabaseUpdateMailrelayGroup(_list);

			UpdateMailrelayGroup updateMailrelayGroup = new UpdateMailrelayGroup(Connection, databaseUpdateMailrelayGroup);
			updateMailrelayGroup.ChangeMailrelayConnection(_mailrelayConnectionTester);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = _groupId });
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>() { status = 1, data = new List<getSubscribersReply>() });
			EnqueueGetSubscribersReply(_contact.new_mailrelaysubscriberid.Value);
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply() { status = 1, data = true });

			updateMailrelayGroup.ExecuteOption(new Administration.Option.Options.OptionReport(typeof(UpdateMailrelayGroupTest)));

			Assert.AreEqual(1, _mailrelayConnectionTester.sendFunctions.Count(function => function.GetType() == typeof(updateSubscriber)));
		}

		[Test]
		public void GroupMemberCanBeRemoved()
		{
			DatabaseUpdateMailrelayGroup databaseUpdateMailrelayGroup = CreateDatabaseUpdateMailrelayGroup(_list);
			int idFromMailrelay = _random.Next(0, int.MaxValue);

			UpdateMailrelayGroup updateMailrelayGroup = new UpdateMailrelayGroup(Connection, databaseUpdateMailrelayGroup);
			updateMailrelayGroup.ChangeMailrelayConnection(_mailrelayConnectionTester);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = _groupId });
			EnqueueGetSubscribersReply(_contact.new_mailrelaysubscriberid.Value, idFromMailrelay);
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply() { status = 1, data = true });

			updateMailrelayGroup.ExecuteOption(new Administration.Option.Options.OptionReport(typeof(UpdateMailrelayGroupTest)));

			updateSubscriber updateSubscriberFunction = (updateSubscriber)_mailrelayConnectionTester.sendFunctions.Single(function => function.GetType() == typeof(updateSubscriber));

			Assert.AreEqual(0, updateSubscriberFunction.groups.Count);
		}

		[TearDown]
		new public void TearDown()
		{
			base.TearDown();

			_contact.Delete();
			_list.Delete();

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}

		private void EnqueueGetSubscribersReply(params int[] contactIds)
		{
			List<getSubscribersReply> replyList = new List<getSubscribersReply>();

			foreach (int id in contactIds)
			{
				replyList.Add(new getSubscribersReply()
				{
					id = id.ToString(),
					groups = new List<string>() { _groupId.ToString() },
				});
			}

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = replyList,
			});
		}

		private DatabaseUpdateMailrelayGroup CreateDatabaseUpdateMailrelayGroup(MarketingList list)
		{
			return new DatabaseUpdateMailrelayGroup()
			{
				listId = list.Id,
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};
		}

		private MarketingList InsertMarketingListOnCampaign(Contact contact)
		{
			MarketingList list = new MarketingList(DynamicsCrmConnection)
			{
				listname = $"test {Guid.NewGuid()}",
				new_controlmailrelaygroup = true,
				createdfrom = MarketingList.createdfromcodeEnum.Contact,
				query =
					new XDocument(
						new XElement("fetch",
							new XElement("entity", new XAttribute("name", "contact"),
								new XElement("attribute", new XAttribute("name", "emailaddress1")),
								new XElement("filter",
									new XElement("condition",
										new XAttribute("attribute", "contactid"),
										new XAttribute("operator", "eq"),
										new XAttribute("value", contact.Id)))))).ToString(),
			};
			list.InsertWithoutRead();
			return list;
		}
	}
}
