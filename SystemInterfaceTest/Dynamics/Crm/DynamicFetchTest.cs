using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class DynamicFetchTest : TestBase
	{
		[Test]
		public void DynamicFetchReturnsData()
		{
			List<dynamic> dynamicEntities = DynamicFetch.ReadFromFetchXml(_dynamicsCrmConnection, "contact", new List<string>() { "contactid" }, new Dictionary<string, string>(), 1, new PagingInformation());

			Assert.AreNotEqual(Guid.Empty, (Guid)dynamicEntities.Single().contactid);
		}
	}
}
