using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using DatabaseExportContactToMailrelay = DataLayer.MongoData.Option.Options.Logic.ExportContactToMailrelay;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class ExportContactToMailrelayTest : TestBase
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

			_list = InsertMarketingListOnContact(_contact);
		}

		[Test]
		public void ContactWithSubscriberIdWillNotBeExported()
		{
			DatabaseExportContactToMailrelay databaseExportContactToMailrelay = CreateDatabaseUpdateMailrelayGroup(_list);

			ExportContactToMailrelay exportContactToMailrelay = new ExportContactToMailrelay(Connection, databaseExportContactToMailrelay);
			exportContactToMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = 1 });

			exportContactToMailrelay.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Assert.AreEqual(1, _mailrelayConnectionTester.sendFunctions.Count);
		}

		[Test]
		public void ContactWithoutSubscriberIdWillBeExported()
		{
			_contact.new_mailrelaysubscriberid = null;
			_contact.Update();
			int newId = _random.Next(0, int.MaxValue);

			DatabaseExportContactToMailrelay databaseExportContactToMailrelay = CreateDatabaseUpdateMailrelayGroup(_list);

			ExportContactToMailrelay exportContactToMailrelay = new ExportContactToMailrelay(Connection, databaseExportContactToMailrelay);
			exportContactToMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = 1 });
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayIntReply() { status = 1, data = newId });

			exportContactToMailrelay.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			MarketingList listRead = MarketingList.GetListForMailrelayUpdate(DynamicsCrmConnection, new PagingInformation(), _list.Id);
			Contact contactRead = Contact.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "contactid", "new_mailrelaysubscriberid" }, new Dictionary<string, string>() { { "contactid", _contact.Id.ToString() } }).Single();

			Assert.AreEqual(1, _mailrelayConnectionTester.sendFunctions.Count(function => function.GetType() == typeof(addSubscriber)));
			Assert.IsNotNull(listRead.new_mailrelaygroupid);
			Assert.AreEqual(newId, contactRead.new_mailrelaysubscriberid);
		}

		private MarketingList InsertMarketingListOnContact(Contact contact)
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

		private DatabaseExportContactToMailrelay CreateDatabaseUpdateMailrelayGroup(MarketingList list)
		{
			return new DatabaseExportContactToMailrelay()
			{
				listId = list.Id,
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};
		}

		[TearDown]
		new public void TearDown()
		{
			base.TearDown();

			_contact.Delete();
			_list.Delete();

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}
	}
}
