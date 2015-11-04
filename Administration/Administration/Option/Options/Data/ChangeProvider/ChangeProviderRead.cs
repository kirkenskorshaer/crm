using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseChangeProviderRead = DataLayer.MongoData.Option.Options.Data.ChangeProvider.ChangeProviderRead;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ChangeProvider
{
	public class ChangeProviderRead : AbstractDataOptionBase
	{
		private DatabaseChangeProviderRead _databaseChangeProviderRead;

		public ChangeProviderRead(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ChangeProviderRead> Find(MongoConnection connection)
		{
			List<DatabaseChangeProviderRead> databaseChangeProviderReads = DatabaseOptionBase.ReadAllowed<DatabaseChangeProviderRead>(connection);

			return databaseChangeProviderReads.Select(databaseChangeProviderRead => new ChangeProviderRead(connection, databaseChangeProviderRead)
			{
				_databaseChangeProviderRead = databaseChangeProviderRead,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			return true;
		}
	}
}
