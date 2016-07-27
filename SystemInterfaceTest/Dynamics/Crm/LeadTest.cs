using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class LeadTest : TestBase
	{
		[Test]
		public void LeadCanHaveSubscriberIdUpdated()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			int subscriberId = new Random().Next(1, int.MaxValue);

			Lead lead = Lead.Create(_dynamicsCrmConnection, new Dictionary<string, string>()
			{
				{ "firstname", firstname}
			});

			lead.InsertWithoutRead();

			Lead.UpdateSubscriberId(_dynamicsCrmConnection, lead.Id, subscriberId);

			Lead leadRead = Lead.ReadFromFetchXml(_dynamicsCrmConnection, new List<string>() { "leadid", "firstname", "new_mailrelaysubscriberid" }, new Dictionary<string, string>() { { "leadid", lead.Id.ToString() } }).Single();

			lead.Delete();

			Assert.AreEqual(firstname, leadRead.firstname);
			Assert.AreEqual(subscriberId, leadRead.new_mailrelaysubscriberid);
		}
	}
}
