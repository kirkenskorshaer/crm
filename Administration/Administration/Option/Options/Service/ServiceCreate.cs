﻿using System.Collections.Generic;
using System.Linq;
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

			bool serverExists = DataLayer.MongoData.Server.Exists(Connection, serviceCreateDatabase.Ip);

			if (serverExists == false)
			{
				Log.Write(Connection, $"Could not find server {serviceCreateDatabase.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				return;
			}

			DataLayer.MongoData.Server server = DataLayer.MongoData.Server.GetServer(Connection, serviceCreateDatabase.Ip);

			bool serviceExists = administration.ServiceExists(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName);

			if (serviceExists)
			{
				Log.Write(Connection, $"Service already existed {serviceCreateDatabase.ServiceName} on ip {server.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
			}
			else
			{
				administration.ServiceCreate(server.Ip, server.Username, server.Password, serviceCreateDatabase.ServiceName, serviceCreateDatabase.Path);
				Log.Write(Connection, $"Created service {serviceCreateDatabase.ServiceName} on ip {server.Ip}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}
		}

		public static List<ServiceCreate> Find(MongoConnection connection)
		{
			List<DataLayer.MongoData.Option.Options.Service.ServiceCreate> options = DataLayer.MongoData.Option.OptionBase.ReadAllowed<DataLayer.MongoData.Option.Options.Service.ServiceCreate>(connection);

			return options.Select(option => new ServiceCreate(connection, option)).ToList();
		}
	}
}