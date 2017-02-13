using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SystemInterface;

namespace SystemInterfaceTest
{
	[TestFixture]
	public class DnsHelperTest : TestBase
	{
		[Test]
		public void GetActiveEthernetIpv4DnsAddressesReturnsAddresses()
		{
			List<IPAddress> ipAddresses = DnsHelper.GetActiveEthernetIpv4DnsAddresses();

			Assert.AreNotEqual(0, ipAddresses.Count);
		}

		[Test]
		[Ignore("")]
		public void SetDnsSetsAValidDns()
		{
			string newIp = "176.28.51.226";
			string name = "Local Area Connection";

			DnsHelper.SetDns(newIp, name);

			List<IPAddress> ipAddresses = DnsHelper.GetActiveEthernetIpv4DnsAddresses();

			DnsHelper.RemoveDns(name);

			Assert.IsTrue(ipAddresses.Any(ip => ip.ToString() == newIp));
		}
	}
}
