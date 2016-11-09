using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SystemInterface.Unwire;

namespace SystemInterfaceTest.UnwireTest
{
	[TestFixture]
	public class UnwireConnectionTest : TestBase
	{
		[Test]
		public void XmlCanBeCreated()
		{
			UnwireConnection unwireConnection = new UnwireConnection("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Unwire/core-1.2.xsd");
			List<Sms> smsList = new List<Sms>();

            XDocument connectionXml = unwireConnection.CreateSendXml(smsList);
		}
	}
}
