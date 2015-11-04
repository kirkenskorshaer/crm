using DataLayer;

namespace Administration.Option.Options.Data
{
	public class MaintainTables : AbstractDataOptionBase
	{
		public MaintainTables(MongoConnection connection) : base(connection, null)
		{
		}

		protected override bool ExecuteOption()
		{
			DataLayer.SqlData.Utilities.MaintainAllTables(SqlConnection);

			return true;
		}
	}
}
