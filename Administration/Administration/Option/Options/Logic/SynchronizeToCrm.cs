using Administration.Option.Options.Data;
using System;
using System.Collections.Generic;
using DatabaseSynchronizeToCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using DataLayer;
using SystemInterface.Dynamics.Crm;
using Administration.Mapping.Contact;

namespace Administration.Option.Options.Logic
{
	public class SynchronizeToCrm : AbstractDataOptionBase
	{
		private DatabaseSynchronizeToCrm _databaseSynchronizeToCrm;
		private DynamicsCrmConnection _dynamicsCrmConnection;
		private SynchronizeFromCrm _synchronizeFromCrm;

		public SynchronizeToCrm(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeToCrm = (DatabaseSynchronizeToCrm)databaseOption;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, "DynamicsCrm");
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			_synchronizeFromCrm = new SynchronizeFromCrm(connection, null);
		}

		protected override bool ExecuteOption()
		{
			Guid changeProviderId = _databaseSynchronizeToCrm.changeProviderId;

			DataLayer.MongoData.Progress progress;
			DatabaseContact databaseContact = GetContactToSynchronize(out progress);

			List<DatabaseExternalContact> externalContacts = ContactCrmMapping.FindContacts(Connection, SqlConnection, databaseContact, changeProviderId);

			if (externalContacts.Count == 0)
			{
				return false;
			}

			externalContacts.ForEach(contact => UpdateExternalContactIfNeeded(changeProviderId, contact, databaseContact));

			progress.UpdateAndSetLastProgressDateToNow(Connection);

			return true;
		}

		private void UpdateExternalContactIfNeeded(Guid changeProviderId, DatabaseExternalContact databaseExternalContact, DatabaseContact databaseContact)
		{
			SystemInterfaceContact systemInterfaceContactInCrm = SystemInterfaceContact.Read(_dynamicsCrmConnection, databaseExternalContact.ExternalContactId);

			SystemInterfaceContact systemInterfaceContact = new SystemInterfaceContact()
			{
				CreatedOn = databaseContact.CreatedOn,
				Firstname = databaseContact.Firstname,
				Lastname = databaseContact.Lastname,
				ModifiedOn = databaseContact.ModifiedOn,

				ContactId = databaseExternalContact.ExternalContactId,
			};

			if (systemInterfaceContactInCrm.Equals(systemInterfaceContact))
			{
				return;
			}

			systemInterfaceContact.SetActive(_dynamicsCrmConnection, false);

			systemInterfaceContactInCrm = SystemInterfaceContact.Read(_dynamicsCrmConnection, databaseExternalContact.ExternalContactId);

			_synchronizeFromCrm.StoreInContactChangesIfNeeded(systemInterfaceContactInCrm, changeProviderId);

			systemInterfaceContact.Update(_dynamicsCrmConnection);

			systemInterfaceContact.SetActive(_dynamicsCrmConnection, true);
		}

		private DatabaseContact GetContactToSynchronize(out DataLayer.MongoData.Progress progress)
		{
			progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressContactToCrm);

			if (progress == null)
			{
				return null;
			}

			Guid contactId = progress.TargetId;

			DatabaseContact contact = DatabaseContact.Read(SqlConnection, contactId);

			return contact;
		}
	}
}
