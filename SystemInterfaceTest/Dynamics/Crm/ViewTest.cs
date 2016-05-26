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
	public class ViewTest : TestBase
	{
		[Test]
		public void Test()
		{
			View.MaintainView(_dynamicsCrmConnection);
		}
	}
}
