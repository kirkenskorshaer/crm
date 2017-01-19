using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseAdjustDns = DataLayer.MongoData.Option.Options.Logic.AdjustDns;
using SystemInterface;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class AdjustDnsTest : TestBase
	{
		[Test]
		[Ignore]
		public void RetreiveDnsReturnsDns()
		{
			DatabaseAdjustDns databaseAdjustDns = new DatabaseAdjustDns()
			{
				adapterName = "Local Area Connection",
				dnsIp = "176.28.51.226",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			AdjustDns adjustDns = new AdjustDns(Connection, databaseAdjustDns);

			Administration.Option.Options.OptionReport report = new Administration.Option.Options.OptionReport("test");
			adjustDns.ExecuteOption(report);
			bool isSuccess = report.Success;

			DnsHelper.RemoveDns(databaseAdjustDns.adapterName);

			Assert.IsTrue(isSuccess);
		}
	}
}
