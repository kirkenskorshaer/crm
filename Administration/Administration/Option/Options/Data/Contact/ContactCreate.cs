using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseContactCreate = DataLayer.MongoData.Option.Options.Data.Contact.ContactCreate;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.Contact
{
	public class ContactCreate : AbstractDataOptionBase
	{
		private DatabaseContactCreate _databaseContactCreate;

		public ContactCreate(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ContactCreate> Find(MongoConnection connection)
		{
			List<DatabaseContactCreate> databaseContactCreates = DatabaseOptionBase.ReadAllowed<DatabaseContactCreate>(connection);

			return databaseContactCreates.Select(databaseContactCreate => new ContactCreate(connection, databaseContactCreate)
			{
				_databaseContactCreate = databaseContactCreate,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			Guid contactId = _databaseContactCreate.ContactId;

			DateTime CreatedOn = _databaseContactCreate.CreatedOn;
			DateTime ModifiedOn = _databaseContactCreate.ModifiedOn;

			string Firstname = _databaseContactCreate.Firstname;
			string Lastname = _databaseContactCreate.Lastname;

			DataLayer.SqlData.Contact.Contact contact = new DataLayer.SqlData.Contact.Contact();

			contact.CreatedOn = CreatedOn;
			contact.ModifiedOn = ModifiedOn;
			contact.Firstname = Firstname;
			contact.Lastname = Lastname;

			contact.Insert(SqlConnection);

			return true;
		}
	}
}
