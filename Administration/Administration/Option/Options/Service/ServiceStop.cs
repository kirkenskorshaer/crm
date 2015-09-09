using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				administration.ServiceStop(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName);
			}
		}
	}
}
