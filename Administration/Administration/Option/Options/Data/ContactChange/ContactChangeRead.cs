using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseContactChangeRead = DataLayer.MongoData.Option.Options.Data.ContactChange.ContactChangeRead;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ContactChange
{
	public class ContactChangeRead : AbstractDataOptionBase
	{
		private DatabaseContactChangeRead _databaseContactChangeRead;

		public ContactChangeRead(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ContactChangeRead> Find(MongoConnection connection)
		{
			List<DatabaseContactChangeRead> databaseContactChangeReads = DatabaseOptionBase.ReadAllowed<DatabaseContactChangeRead>(connection);

			return databaseContactChangeReads.Select(databaseContactChangeRead => new ContactChangeRead(connection, databaseContactChangeRead)
			{
				_databaseContactChangeRead = databaseContactChangeRead,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			return true;
		}
	}
}
