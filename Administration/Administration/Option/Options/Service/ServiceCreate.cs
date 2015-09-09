using SystemInterface;
using DataLayer;

namespace Administration.Option.Options.Service
{
	public class ServiceCreate : OptionBase
	{
		public ServiceCreate(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		protected override void ExecuteOption()
		{
			RemoteAdministration administration = new RemoteAdministration();

			DataLayer.MongoData.Option.Options.Service.ServiceCreate serviceCreateDatabase = (DataLayer.MongoData.Option.Options.Service.ServiceCreate)DatabaseOption;

			DataLayer.MongoData.Server server = DataLayer.MongoData.Server.GetServer(Connection, serviceCreateDatabase.Ip);

			bool serviceExists = administration.ServiceExists(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName);

			if (serviceExists == false)
			{
				administration.ServiceCreate(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName, serviceCreateDatabase.Path);
			}
		}
	}
}
