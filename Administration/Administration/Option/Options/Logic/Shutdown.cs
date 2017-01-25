using DataLayer;
using DatabaseShutdown = DataLayer.MongoData.Option.Options.Logic.Shutdown;

namespace Administration.Option.Options.Logic
{
	public class Shutdown : OptionBase
	{
		private DatabaseShutdown _databaseShutdown;

		public Shutdown(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseShutdown = (DatabaseShutdown)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			report.Success = true;
		}
	}
}
