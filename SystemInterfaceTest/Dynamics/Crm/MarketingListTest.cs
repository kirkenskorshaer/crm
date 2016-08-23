using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class MarketingListTest
	{
		private DynamicsCrmConnection _connection;

		[SetUp]
		public void SetUp()
		{
			MongoConnection connection = MongoConnection.GetConnection("test");
			UrlLogin login = UrlLogin.GetUrlLogin(connection, "test");
			_connection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
		}

		[Test]
		public void InsertedAreSameAsRead()
		{
			MarketingList marketingListInserted = InsertMarketingList();

			MarketingList marketingListRead = MarketingList.Read(_connection, marketingListInserted.Id);

			marketingListInserted.Delete();

			Assert.AreEqual(marketingListInserted.createdfrom, marketingListRead.createdfrom);
			Assert.AreEqual(marketingListInserted.listname, marketingListRead.listname);
			Assert.AreEqual(marketingListInserted.Id, marketingListRead.Id);
			Assert.AreEqual(marketingListInserted.query, marketingListRead.query);
		}

		[Test]
		public void DeleteRemovesInserted()
		{
			MarketingList marketingListInserted = InsertMarketingList();

			marketingListInserted.Delete();

			TestDelegate readTest = () => MarketingList.Read(_connection, marketingListInserted.Id);

			Assert.Throws(Is.InstanceOf(typeof(Exception)), readTest);
		}

		[Test]
		[Ignore]
		public void CrmIdsAndSubscriberIdsForTestList()
		{
			MarketingList list = MarketingList.Read(_connection, Guid.Parse("b70001ba-1569-e611-80f7-001c4215c4a0"));

			foreach (KeyValueEntity<Guid, int?> crmAndSubscriber in list.CrmIdsAndSubscriberIds)
			{
				Contact contact = Contact.ReadFromFetchXml(_connection, new List<string>() { "emailaddress1", "new_mailrelaysubscriberid" }, new Dictionary<string, string>() { { "contactid", crmAndSubscriber.key.ToString() } }).Single();
				Console.Out.WriteLine($"{contact.emailaddress1} - {contact.new_mailrelaysubscriberid}");
			}
		}

		[Test]
		[Ignore]
		public void ContentIdsForNonMailrelaySubscribersTestList()
		{
			MarketingList list = MarketingList.Read(_connection, Guid.Parse("b70001ba-1569-e611-80f7-001c4215c4a0"));

			foreach (Guid crmId in list.ContentIdsForNonMailrelaySubscribers)
			{
				Contact contact = Contact.ReadFromFetchXml(_connection, new List<string>() { "emailaddress1", "new_mailrelaysubscriberid" }, new Dictionary<string, string>() { { "contactid", crmId.ToString() } }).Single();
				Console.Out.WriteLine($"{contact.emailaddress1} - {contact.new_mailrelaysubscriberid}");
			}
		}

		[Test]
		[Ignore]
		public void GetListForMailrelayUpdateOnlyReturnsListThatShouldBeImported()
		{
			PagingInformation pagingInformation = new PagingInformation();

			while (pagingInformation.FirstRun || pagingInformation.MoreRecords)
			{
				MarketingList marketingList = MarketingList.GetListForMailrelayUpdate(_connection, pagingInformation, null);

				if (marketingList != null)
				{
					Console.Out.WriteLine($" {marketingList.Id} {marketingList.new_controlmailrelaygroup} - {marketingList.new_mailrelaygroupid} - {marketingList.new_mailrelaycheck}");
				}
			}
		}

		private MarketingList InsertMarketingList()
		{
			MarketingList marketingListInserted = new MarketingList(_connection);

			marketingListInserted.listname = $"testList_{DateTime.Now.ToString("yyyyMMdd_HH:mm:ss")}";

			marketingListInserted.query = @"
				<fetch mapping='logical' distinct='true'>
					<entity name='contact'>
						<attribute name='firstname' />
						<order attribute='firstname' descending='false' />
						<link-entity name='new_group_contact' from='contactid' to='contactid' visible='false' intersect='true'>
							<link-entity name='new_group' from='new_groupid' to='new_groupid' visible='false' intersect='true'>
								<attribute name='new_name' />
								<filter type='and'>
									<condition attribute='new_name' operator='eq' value='Støtten' />
								</filter>
							</link-entity>
						</link-entity>
						<link-entity name='new_group_contact' from='contactid' to='contactid' visible='false' intersect='true'>
							<link-entity name='new_group' from='new_groupid' to='new_groupid' visible='false' intersect='true'>
								<attribute name='new_name' />
								<filter type='and'>
									<condition attribute='new_name' operator='eq' value='Årsmærker' />
								</filter>
							</link-entity>
						</link-entity>
					</entity>
				</fetch>";

			marketingListInserted.InsertWithoutRead();
			return marketingListInserted;
		}
	}
}
