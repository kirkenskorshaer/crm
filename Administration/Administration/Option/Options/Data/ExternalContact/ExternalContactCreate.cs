using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseExternalContactCreate = DataLayer.MongoData.Option.Options.Data.ExternalContact.ExternalContactCreate;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Data.ExternalContact
{
	public class ExternalContactCreate : AbstractDataOptionBase
	{
		private DatabaseExternalContactCreate _databaseExternalContactCreate;

		public ExternalContactCreate(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<ExternalContactCreate> Find(MongoConnection connection)
		{
			List<DatabaseExternalContactCreate> databaseExternalContactCreates = DatabaseOptionBase.ReadAllowed<DatabaseExternalContactCreate>(connection);

			return databaseExternalContactCreates.Select(databaseExternalContactCreate => new ExternalContactCreate(connection, databaseExternalContactCreate)
			{
				_databaseExternalContactCreate = databaseExternalContactCreate,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			Guid externalContactId = _databaseExternalContactCreate.ExternalContactId;
			Guid changeProviderId = _databaseExternalContactCreate.ChangeProviderId;
			Guid contactId = _databaseExternalContactCreate.ContactId;

			DataLayer.SqlData.Contact.ExternalContact externalContact = new DataLayer.SqlData.Contact.ExternalContact(SqlConnection, externalContactId, changeProviderId, contactId);
			externalContact.Insert();

			return true;
		}
	}
}
