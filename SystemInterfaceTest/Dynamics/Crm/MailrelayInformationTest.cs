using NUnit.Framework;
using System;
using System.Collections.Generic;
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

		[Test]
		public void GetMailrelayFromContactCanReturnInformation()
		{
			PagingInformation pagingInformation = new PagingInformation();

			int subscriberId = new Random().Next(1, int.MaxValue);

			Contact contact = CreateTestContact();
			contact.new_mailrelaysubscriberid = subscriberId;
			contact.InsertWithoutRead();

			List<MailrelayInformation> informations = MailrelayInformation.GetMailrelayFromContact(_dynamicsCrmConnection, _config.GetResourcePath, pagingInformation, 1, contact.Id);

			contact.Delete();

			Assert.AreEqual(1, informations.Count);
		}
	}
}
