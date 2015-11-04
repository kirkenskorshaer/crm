using DataLayer;
using System.Data.SqlClient;
using DatabaseOptionType = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data
{
	public abstract class AbstractDataOptionBase : OptionBase
	{
		protected SqlConnection SqlConnection;

		public AbstractDataOptionBase(MongoConnection connection, DatabaseOptionType databaseOption) : base(connection, databaseOption)
		{
			SqlConnection = SqlConnectionHolder.GetConnection(connection, "sql");
		}
	}
}
