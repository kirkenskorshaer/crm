using DataLayer;
using System.Collections.Generic;
using DatabaseOption = DataLayer.MongoData.Option.OptionBase;

namespace WebAdministration.Option
{
	public abstract class OptionBase
	{
		protected DatabaseOption _databaseOption;
		protected MongoConnection _mongoConnection;

		protected OptionBase(MongoConnection mongoConnection, DatabaseOption databaseOption)
		{
			_mongoConnection = mongoConnection;
			_databaseOption = databaseOption;
		}

		public abstract Dictionary<string, object> ExecuteOption();
	}
}
