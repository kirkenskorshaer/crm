using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseExternalContactRead = DataLayer.MongoData.Option.Options.Data.ExternalContact.ExternalContactRead;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ExternalContact
{
	public class ExternalContactRead : AbstractDataOptionBase
	{
		private DatabaseExternalContactRead _databaseExternalContactRead;

		public ExternalContactRead(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ExternalContactRead> Find(MongoConnection connection)
		{
			List<DatabaseExternalContactRead> databaseExternalContactReads = DatabaseOptionBase.ReadAllowed<DatabaseExternalContactRead>(connection);

			return databaseExternalContactReads.Select(databaseExternalContactRead => new ExternalContactRead(connection, databaseExternalContactRead)
			{
				_databaseExternalContactRead = databaseExternalContactRead,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			return true;
		}
	}
}
