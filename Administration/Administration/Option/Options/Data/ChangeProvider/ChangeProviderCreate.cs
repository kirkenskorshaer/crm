using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseChangeProviderCreate = DataLayer.MongoData.Option.Options.Data.ChangeProvider.ChangeProviderCreate;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ChangeProvider
{
	public class ChangeProviderCreate : AbstractDataOptionBase
	{
		private DatabaseChangeProviderCreate _databaseChangeProviderCreate;

		public ChangeProviderCreate(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ChangeProviderCreate> Find(MongoConnection connection)
		{
			List<DatabaseChangeProviderCreate> databaseChangeProviderCreates = DatabaseOptionBase.ReadAllowed<DatabaseChangeProviderCreate>(connection);

			return databaseChangeProviderCreates.Select(databaseChangeProviderCreate => new ChangeProviderCreate(connection, databaseChangeProviderCreate)
			{
				_databaseChangeProviderCreate = databaseChangeProviderCreate,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			string providerName = _databaseChangeProviderCreate.ProviderName;

			DataLayer.SqlData.ChangeProvider changeProvider = new DataLayer.SqlData.ChangeProvider();

			changeProvider.Name = providerName;

			changeProvider.Insert(SqlConnection);

			return true;
		}
	}
}
