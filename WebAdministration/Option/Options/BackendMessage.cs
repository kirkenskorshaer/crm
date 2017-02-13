using DataLayer;
using System;
using DatabaseBackendMessage = DataLayer.MongoData.Option.Options.Logic.BackendMessage;
using System.Collections.Generic;

namespace WebAdministration.Option.Options
{
	public class BackendMessage : OptionBase
	{
		private DatabaseBackendMessage _databaseBackendMessage;

		public BackendMessage(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseBackendMessage = (DatabaseBackendMessage)databaseOption;
		}

		public override Dictionary<string, object> ExecuteOption()
		{
			Dictionary<string, object> returnParameters = new Dictionary<string, object>();

			returnParameters.Add("test", Guid.NewGuid());

			return returnParameters;
		}
	}
}
