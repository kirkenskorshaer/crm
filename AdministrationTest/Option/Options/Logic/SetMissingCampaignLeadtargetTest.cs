using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseSetMissingCampaignLeadtarget = DataLayer.MongoData.Option.Options.Logic.SetMissingCampaignLeadtarget;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SetMissingCampaignLeadtargetTest : TestBase
	{
		[Test]
		public void test()
		{
			DatabaseSetMissingCampaignLeadtarget databaseSetMissingCampaignLeadtarget = CreateDatabaseSetMissingCampaignLeadtarget();

			SetMissingCampaignLeadtarget setMissingCampaignLeadtarget = new SetMissingCampaignLeadtarget(Connection, databaseSetMissingCampaignLeadtarget);

			setMissingCampaignLeadtarget.Execute();
		}

		private DatabaseSetMissingCampaignLeadtarget CreateDatabaseSetMissingCampaignLeadtarget()
		{
			return new DatabaseSetMissingCampaignLeadtarget()
			{
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
				baseUrl = "https://webinterfacetest.korsnet.dk/stub.aspx",
			};
		}
	}
}