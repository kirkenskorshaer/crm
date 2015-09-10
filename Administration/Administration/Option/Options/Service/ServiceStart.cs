using System.Collections.Generic;
using System.Linq;
using SystemInterface;
using DataLayer;

namespace Administration.Option.Options.Service
{
	public class ServiceStart : OptionBase
	{
		public ServiceStart(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		protected override void ExecuteOption()
		{
			RemoteAdministration administration = new RemoteAdministration();

			DataLayer.MongoData.Option.Options.Service.ServiceStart serviceStartDatabase = (DataLayer.MongoData.Option.Options.Service.ServiceStart)DatabaseOption;

			bool serverExists = DataLayer.MongoData.Server.Exists(Connection, serviceStartDatabase.Ip);

			if (serverExists == false)
			{
				Log.Write(Connection, $"Could not find server {serviceStartDatabase.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				return;
			}

			DataLayer.MongoData.Server server = DataLayer.MongoData.Server.GetServer(Connection, serviceStartDatabase.Ip);

			bool serviceExists = administration.ServiceExists(server.Ip, server.Username, server.Password, serviceStartDatabase.ServiceName);

			if (serviceExists == false)
			{
				administration.ServiceStart(server.Ip, server.Username, server.Password, serviceStartDatabase.ServiceName);
				Log.Write(Connection, $"Started service {serviceStartDatabase.ServiceName} on ip {server.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}
			else
			{
				Log.Write(Connection, $"Service not found {serviceStartDatabase.ServiceName} on ip {server.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
			}
		}

		public static List<ServiceStart> Find(MongoConnection connection)
		{
			List<DataLayer.MongoData.Option.Options.Service.ServiceStart> options = DataLayer.MongoData.Option.OptionBase.ReadAllowed<DataLayer.MongoData.Option.Options.Service.ServiceStart>(connection);

			return options.Select(option => new ServiceStart(connection, option)).ToList();
		}
	}
}
