using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseMaintainAllTables = DataLayer.MongoData.Option.Options.Data.Utilities.MaintainAllTables;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.Utilities
{
	public class MaintainAllTables : AbstractDataOptionBase
	{
		private DatabaseMaintainAllTables _databaseMaintainAllTables;

		public MaintainAllTables(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<MaintainAllTables> Find(MongoConnection connection)
		{
			List<DatabaseMaintainAllTables> databaseMaintainAllTabless = DatabaseOptionBase.ReadAllowed<DatabaseMaintainAllTables>(connection);

			return databaseMaintainAllTabless.Select(databaseMaintainAllTables => new MaintainAllTables(connection, databaseMaintainAllTables)
			{
				_databaseMaintainAllTables = databaseMaintainAllTables,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			DataLayer.SqlData.SqlUtilities.MaintainAllTables(SqlConnection);

			return true;
		}
	}
}
