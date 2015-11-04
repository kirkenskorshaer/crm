using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseContactRead = DataLayer.MongoData.Option.Options.Data.Contact.ContactRead;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.Contact
{
	public class ContactRead : AbstractDataOptionBase
	{
		private DatabaseContactRead _databaseContactRead;

		public ContactRead(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ContactRead> Find(MongoConnection connection)
		{
			List<DatabaseContactRead> databaseContactReads = DatabaseOptionBase.ReadAllowed<DatabaseContactRead>(connection);

			return databaseContactReads.Select(databaseContactRead => new ContactRead(connection, databaseContactRead)
			{
				_databaseContactRead = databaseContactRead,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			return true;
		}
	}
}
