﻿namespace DataLayer.MongoData.Option.Options.Service
{
	public class ServiceDelete: OptionBase
	{
		public string Ip { get; set; }
		public string ServiceName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ServiceDelete>(connection);
			}
			else
			{
				Delete<ServiceDelete>(connection);
			}
		}
	}
}
