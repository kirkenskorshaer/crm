using System.Collections.Generic;
using System.Linq;
using DataLayer;
using System.Net;
using SystemInterface;
using DatabaseAdjustDns = DataLayer.MongoData.Option.Options.Logic.AdjustDns;

namespace Administration.Option.Options.Logic
{
	public class AdjustDns : OptionBase
	{
		private DatabaseAdjustDns _databaseAdjustDns;

		public AdjustDns(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseAdjustDns = (DatabaseAdjustDns)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			string dns = _databaseAdjustDns.dnsIp;
			string name = _databaseAdjustDns.adapterName;

			DnsHelper.SetDns(dns, name);

			List<IPAddress> addresses = DnsHelper.GetActiveEthernetIpv4DnsAddresses();

			report.Success = addresses.Any(address => address.ToString() == dns);

			return;
		}
	}
}
