using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
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

			MarketingList marketingListRead = MarketingList.Read(_connection, marketingListInserted.listid);

			marketingListInserted.Delete(_connection);

			Assert.AreEqual(marketingListInserted.createdfromcode, marketingListRead.createdfromcode);
			Assert.AreEqual(marketingListInserted.listname, marketingListRead.listname);
			Assert.AreEqual(marketingListInserted.listid, marketingListRead.listid);
			Assert.AreEqual(marketingListInserted.query, marketingListRead.query);
		}

		[Test]
		public void DeleteRemovesInserted()
		{
			MarketingList marketingListInserted = InsertMarketingList();

			marketingListInserted.Delete(_connection);

			TestDelegate readTest = () => MarketingList.Read(_connection, marketingListInserted.listid);

			Assert.Throws(Is.InstanceOf(typeof(Exception)), readTest);
		}

		private MarketingList InsertMarketingList()
		{
			MarketingList marketingListInserted = new MarketingList();

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

			marketingListInserted.Insert(_connection);
			return marketingListInserted;
		}
	}
}
