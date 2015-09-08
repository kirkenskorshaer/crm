using DataLayer;
using DatabaseOptionType = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option
{
	public abstract class OptionBase
	{
		protected readonly MongoConnection Connection;
		protected readonly DataLayer.MongoData.Config Config;
		protected readonly DatabaseOptionType DatabaseOption;

		protected OptionBase(MongoConnection connection, DatabaseOptionType databaseOption)
		{
			Connection = connection;
			Config = DataLayer.MongoData.Config.GetConfig(Connection);
			DatabaseOption = databaseOption;
		}

		protected abstract void ExecuteOption();

		public void Execute()
		{
			ExecuteOption();

			DatabaseOption?.Execute(Connection);
		}
	}
}
