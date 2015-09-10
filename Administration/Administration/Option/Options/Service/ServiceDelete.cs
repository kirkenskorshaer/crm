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

			DataLayer.MongoData.Option.Options.Service.ServiceDelete serviceDeleteDatabase = (DataLayer.MongoData.Option.Options.Service.ServiceDelete)DatabaseOption;

			bool serverExists = DataLayer.MongoData.Server.Exists(Connection, serviceDeleteDatabase.Ip);

			if (serverExists == false)
			{
				Log.Write(Connection, $"Could not find server {serviceDeleteDatabase.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				return;
			}

			DataLayer.MongoData.Server server = DataLayer.MongoData.Server.GetServer(Connection, serviceDeleteDatabase.Ip);

			bool serviceExists = administration.ServiceExists(server.Ip, server.Username, server.Password, serviceDeleteDatabase.ServiceName);

			if (serviceExists)
			{
				administration.ServiceDelete(server.Ip, server.Username, server.Password, serviceDeleteDatabase.ServiceName);
				Log.Write(Connection, $"Deleted service {serviceDeleteDatabase.ServiceName} on ip {server.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}
			else
			{
				Log.Write(Connection, $"Service not found {serviceDeleteDatabase.ServiceName} on ip {server.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
			}
		}

		public static List<ServiceDelete> Find(MongoConnection connection)
		{
			List<DataLayer.MongoData.Option.Options.Service.ServiceDelete> options = DataLayer.MongoData.Option.OptionBase.ReadAllowed<DataLayer.MongoData.Option.Options.Service.ServiceDelete>(connection);

			return options.Select(option => new ServiceDelete(connection, option)).ToList();
		}
	}
}
