using System.Collections.Generic;
using System.Linq;
using SystemInterface;
using DataLayer;

namespace Administration.Option.Options.Service
{
	public class ServiceDelete : OptionBase
	{
		public ServiceDelete(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption)
			: base(connection, databaseOption)
		{
		}

		protected override void ExecuteOption()
		{
			RemoteAdministration administration = new RemoteAdministration();

			DataLayer.MongoData.Option.Options.Service.ServiceCreate serviceCreateDatabase = (DataLayer.MongoData.Option.Options.Service.ServiceCreate)DatabaseOption;

			bool serverExists = DataLayer.MongoData.Server.Exists(Connection, serviceCreateDatabase.Ip);

			if (serverExists == false)
			{
				return;
			}

			DataLayer.MongoData.Server server = DataLayer.MongoData.Server.GetServer(Connection, serviceCreateDatabase.Ip);

			bool serviceExists = administration.ServiceExists(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName);

			if (serviceExists == false)
			{
				administration.ServiceDelete(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName);
			}
		}

		public static List<ServiceDelete> Find(MongoConnection connection)
		{
			List<DataLayer.MongoData.Option.Options.Service.ServiceDelete> options = DataLayer.MongoData.Option.OptionBase.ReadAllowed<DataLayer.MongoData.Option.Options.Service.ServiceDelete>(connection);

			return options.Select(option => new ServiceDelete(connection, option)).ToList();
		}
	}
}
