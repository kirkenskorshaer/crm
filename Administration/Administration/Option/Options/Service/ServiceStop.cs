using System.Collections.Generic;
using System.Linq;
using SystemInterface;
using DataLayer;

namespace Administration.Option.Options.Service
{
	public class ServiceStop : OptionBase
	{
		public ServiceStop(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		protected override void ExecuteOption()
		{
			RemoteAdministration administration = new RemoteAdministration();

			DataLayer.MongoData.Option.Options.Service.ServiceStop serviceStopDatabase = (DataLayer.MongoData.Option.Options.Service.ServiceStop)DatabaseOption;

			bool serverExists = DataLayer.MongoData.Server.Exists(Connection, serviceStopDatabase.Ip);

			if (serverExists == false)
			{
				return;
			}

			DataLayer.MongoData.Server server = DataLayer.MongoData.Server.GetServer(Connection, serviceStopDatabase.Ip);

			bool serviceExists = administration.ServiceExists(server.Ip, server.Username, server.Password, serviceStopDatabase.ServiceName);

			if (serviceExists == false)
			{
				administration.ServiceStop(server.Ip, server.Username, server.Password, serviceStopDatabase.ServiceName);
			}
		}

		public static List<ServiceStop> Find(MongoConnection connection)
		{
			List<DataLayer.MongoData.Option.Options.Service.ServiceStop> options = DataLayer.MongoData.Option.OptionBase.ReadAllowed<DataLayer.MongoData.Option.Options.Service.ServiceStop>(connection);

			return options.Select(option => new ServiceStop(connection, option)).ToList();
		}
	}
}
