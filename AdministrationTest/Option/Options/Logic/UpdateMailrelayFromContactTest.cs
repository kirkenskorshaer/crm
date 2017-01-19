using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using DatabaseUpdateMailrelayFromContact = DataLayer.MongoData.Option.Options.Logic.UpdateMailrelayFromContact;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class UpdateMailrelayFromContactTest : TestBase
	{
		[Test]
		public void UpdateMailRelayUpdatesIfNeeded()
		{
			Contact contact = InsertContact();

			DatabaseUpdateMailrelayFromContact databaseUpdateMailrelayFromContact = CreateDatabaseUpdateMailrelayFromContact(contact);

			UpdateMailrelayFromContact UpdateMailrelayFromContact = new UpdateMailrelayFromContact(Connection, databaseUpdateMailrelayFromContact);
			UpdateMailrelayFromContact.ChangeMailrelayConnection(_mailrelayConnectionTester);

			EnqueueGetSubscriberUpdateSubscriberReply();

			UpdateMailrelayFromContact.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Console.Out.WriteLine(_mailrelayConnectionTester);

			contact.Delete();
		}

		[Test]
		public void UpdateMailRelayDoesNotUpdateIfNotNeeded()
		{
			Contact contact = InsertContact();

			DatabaseUpdateMailrelayFromContact databaseUpdateMailrelayFromContact = CreateDatabaseUpdateMailrelayFromContact(contact);

			UpdateMailrelayFromContact UpdateMailrelayFromContact = new UpdateMailrelayFromContact(Connection, databaseUpdateMailrelayFromContact);
			UpdateMailrelayFromContact.ChangeMailrelayConnection(_mailrelayConnectionTester);

			EnqueueGetSubscriberUpdateSubscriberReply();

			UpdateMailrelayFromContact.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			_mailrelayConnectionTester = new TestUtilities.MailrelayConnectionTester();

			EnqueueGetSubscribers();

			UpdateMailrelayFromContact.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Console.Out.WriteLine(_mailrelayConnectionTester);

			contact.Delete();
		}

		[Test]
		public void UpdateMailRelayUpdatesContact()
		{
			Contact contact = InsertContact();

			DatabaseUpdateMailrelayFromContact databaseUpdateMailrelayFromContact = CreateDatabaseUpdateMailrelayFromContact(contact);

			UpdateMailrelayFromContact UpdateMailrelayFromContact = new UpdateMailrelayFromContact(Connection, databaseUpdateMailrelayFromContact);
			UpdateMailrelayFromContact.ChangeMailrelayConnection(_mailrelayConnectionTester);

			EnqueueGetSubscriberUpdateSubscriberReply();

			UpdateMailrelayFromContact.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Contact contactRead = Contact.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "new_mailrelaysubscriberid", "new_mailrelaycheck" }, new Dictionary<string, string>() { { "contactid", contact.Id.ToString() } }).Single();

			Assert.AreEqual(contact.new_mailrelaysubscriberid, contactRead.new_mailrelaysubscriberid);
			Assert.IsFalse(string.IsNullOrWhiteSpace(contactRead.new_mailrelaycheck));

			contact.Delete();
		}

		private void EnqueueGetSubscribers()
		{
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayArrayReply<getSubscribersReply>()
			{
				status = 1,
				data = new List<getSubscribersReply>()
			{
				new getSubscribersReply()
				{
					email = "email",
					name = "fullname",
					fields = new Dictionary<string, string>(),
					groups = new List<string>(),
				},
			}
			});
		}

		private void EnqueueUpdateSubscriber()
		{
			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply() { status = 1, data = true, });
		}

		private void EnqueueGetSubscriberUpdateSubscriberReply()
		{
			EnqueueGetSubscribers();
			EnqueueUpdateSubscriber();
		}

		private DatabaseUpdateMailrelayFromContact CreateDatabaseUpdateMailrelayFromContact(Contact contact)
		{
			return new DatabaseUpdateMailrelayFromContact()
			{
				Name = "test",
				pageSize = 1,
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
				contactId = contact.Id,
			};
		}

		private Contact InsertContact()
		{
			Contact contact = new Contact(DynamicsCrmConnection)
			{
				firstname = "test",
				lastname = "test",
				new_mailrelaysubscriberid = new Random().Next(0, int.MaxValue),
			};

			contact.InsertWithoutRead();
			return contact;
		}
	}
}
