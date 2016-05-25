using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class CampaignTest : TestBase
	{
		[Test]
		public void ReadLatestTest()
		{
			DateTime testDate = DateTime.Now;
			Campaign campaignInserted1 = CreateTestCampaign(testDate);
			campaignInserted1.Insert();
			Campaign campaignInserted2 = CreateTestCampaign(testDate);
			campaignInserted2.Insert();

			List<Campaign> campaigns = Campaign.ReadLatest(_dynamicsCrmConnection, testDate);

			campaignInserted1.Delete();
			campaignInserted2.Delete();

			Assert.AreEqual(2, campaigns.Count);
		}
	}
}
