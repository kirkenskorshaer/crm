using NUnit.Framework;
using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class CampaignTest : TestBase
	{
		[Test]
		public void ThereAreCampaigns()
		{
			List<Campaign> campaigns = Campaign.ReadAllCampaignIds(_dynamicsCrmConnection);

			Assert.Greater(campaigns.Count, 0);
		}
	}
}
