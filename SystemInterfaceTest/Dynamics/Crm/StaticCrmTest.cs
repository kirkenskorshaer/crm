using NUnit.Framework;
using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class StaticCrmTest : TestBase
	{
		[Test]
		public void GetAllAttributeNames()
		{
			List<string> attributeNames = StaticCrm.GetAllAttributeNames(_dynamicsCrmConnection, typeof(Annotation));

			Assert.Greater(attributeNames.Count, 3);
		}
	}
}
