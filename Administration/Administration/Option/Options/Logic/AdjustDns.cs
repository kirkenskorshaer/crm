using Administration.Option.Options.Data;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using System.Net;
using SystemInterface;
using DatabaseAdjustDns = DataLayer.MongoData.Option.Options.Logic.AdjustDns;

namespace Administration.Option.Options.Logic
{
	public class AdjustDns : AbstractDataOptionBase
	{
		private DatabaseAdjustDns _databaseAdjustDns;

		public AdjustDns(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseAdjustDns = (DatabaseAdjustDns)databaseOption;
		}

		protected override bool ExecuteOption()
		{
			string dns = _databaseAdjustDns.dnsIp;
			string name = _databaseAdjustDns.adapterName;

			DnsHelper.SetDns(dns, name);

			List<IPAddress> addresses = DnsHelper.GetActiveEthernetIpv4DnsAddresses();

			bool success = addresses.Any(address => address.ToString() == dns);

			return success;
		}

		public static List<AdjustDns> Find(MongoConnection connection)
		{
			List<DatabaseAdjustDns> options = DatabaseAdjustDns.ReadAllowed<DatabaseAdjustDns>(connection);

			return options.Select(option => new AdjustDns(connection, option)).ToList();
		}
	}
}
