using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace SystemInterface
{
	public static class DnsHelper
	{
		public static void RemoveDns(string name)
		{
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo("netsh", $"interface ip set dns \"{name}\" source = dhcp")
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
				},
			};
			process.Start();
			process.WaitForExit();

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			if (string.IsNullOrWhiteSpace(output) == false || string.IsNullOrWhiteSpace(error) == false)
			{
				throw new Exception(output + Environment.NewLine + error);
			}
		}

		public static void SetDns(string dns, string name)
		{
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo("netsh", $"interface ip add dns \"{name}\" {dns}")
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
				},
			};

			process.Start();
			process.WaitForExit();

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			if (string.IsNullOrWhiteSpace(output) == false || string.IsNullOrWhiteSpace(error) == false)
			{
				throw new Exception(output + Environment.NewLine + error);
			}
		}

		public static List<IPAddress> GetActiveEthernetIpv4DnsAddresses()
		{
			IEnumerable<NetworkInterface> adapters = NetworkInterface.GetAllNetworkInterfaces();
			adapters = adapters.Where(adapter =>
				adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
				adapter.OperationalStatus == OperationalStatus.Up);

			IEnumerable<IPInterfaceProperties> adapterProperties = adapters.Select(adapter => adapter.GetIPProperties());
			IEnumerable<IPAddress> dnsServers = adapterProperties.SelectMany(adapterProperty => adapterProperty.DnsAddresses);

			dnsServers = dnsServers.Where(dns => dns.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

			return dnsServers.ToList();
		}
	}
}
