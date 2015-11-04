using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseContactChangeCreate = DataLayer.MongoData.Option.Options.Data.ContactChange.ContactChangeCreate;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ContactChange
{
	public class ContactChangeCreate : AbstractDataOptionBase
	{
		private DatabaseContactChangeCreate _databaseContactChangeCreate;

		public ContactChangeCreate(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ContactChangeCreate> Find(MongoConnection connection)
		{
			List<DatabaseContactChangeCreate> databaseContactChangeCreates = DatabaseOptionBase.ReadAllowed<DatabaseContactChangeCreate>(connection);

			return databaseContactChangeCreates.Select(databaseContactChangeCreate => new ContactChangeCreate(connection, databaseContactChangeCreate)
			{
				_databaseContactChangeCreate = databaseContactChangeCreate,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			Guid contactId = _databaseContactChangeCreate.ContactId;
			Guid externalContactId = _databaseContactChangeCreate.ExternalContactId;
			Guid changeProviderId = _databaseContactChangeCreate.ChangeProviderId;

			DateTime CreatedOn = _databaseContactChangeCreate.CreatedOn;
			DateTime ModifiedOn = _databaseContactChangeCreate.ModifiedOn;

			string Firstname = _databaseContactChangeCreate.Firstname;
			string Lastname = _databaseContactChangeCreate.Lastname;

			DataLayer.SqlData.Contact.ContactChange contactChange = new DataLayer.SqlData.Contact.ContactChange(SqlConnection, contactId, externalContactId, changeProviderId);

			contactChange.CreatedOn = CreatedOn;
			contactChange.ModifiedOn = ModifiedOn;
			contactChange.Firstname = Firstname;
			contactChange.Lastname = Lastname;

			return true;
		}
	}
}
