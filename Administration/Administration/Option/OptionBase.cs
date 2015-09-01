using DataLayer;


namespace Administration.Option
{
	public abstract class OptionBase
	{
		protected readonly MongoConnection Connection;
		protected readonly DataLayer.MongoData.Config Config;

		protected OptionBase(MongoConnection connection)
		{
			Connection = connection;
			Config = DataLayer.MongoData.Config.GetConfig(Connection);
		}

		public abstract void Execute();
	}
}
