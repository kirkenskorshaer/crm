using NUnit.Framework;
using System;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class MailrelayInformationTest : TestBase
	{
		[Test]
		public void GetMailrelayFromLeadReturnsALead()
		{
			string email = "test";
			Guid campaignId = Guid.Parse("ff052597-5538-e611-80ef-001c4215c4a0");

			MailrelayInformation information = MailrelayInformation.GetMailrelayFromLead(_dynamicsCrmConnection, _config.GetResourcePath, email, campaignId);

			Assert.IsNotNull(information);
		}
    }
}
