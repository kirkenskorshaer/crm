using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseChangeProviderReadAll = DataLayer.MongoData.Option.Options.Data.ChangeProvider.ChangeProviderReadAll;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ChangeProvider
{
	public class ChangeProviderReadAll : AbstractDataOptionBase
	{
		private DatabaseChangeProviderReadAll _databaseChangeProviderReadAll;

		public ChangeProviderReadAll(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ChangeProviderReadAll> Find(MongoConnection connection)
		{
			List<DatabaseChangeProviderReadAll> databaseChangeProviderReadAlls = DatabaseOptionBase.ReadAllowed<DatabaseChangeProviderReadAll>(connection);

			return databaseChangeProviderReadAlls.Select(databaseChangeProviderReadAll => new ChangeProviderReadAll(connection, databaseChangeProviderReadAll)
			{
				_databaseChangeProviderReadAll = databaseChangeProviderReadAll,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			return true;
		}
	}
}
