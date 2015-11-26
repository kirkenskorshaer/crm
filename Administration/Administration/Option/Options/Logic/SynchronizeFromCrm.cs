using Administration.Option.Options.Data;
using System;
using DatabaseSynchronizeFromCrm = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DataLayer;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using Administration.Mapping.Contact;
using System.Data.SqlClient;

namespace Administration.Option.Options.Logic
{
	public class SynchronizeFromCrm : AbstractDataOptionBase
	{
		private DatabaseSynchronizeFromCrm _databaseSynchronizeFromCrm;

		public SynchronizeFromCrm(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeFromCrm = (DatabaseSynchronizeFromCrm)databaseOption;
		}

		public static List<SynchronizeFromCrm> Find(MongoConnection connection)
		{
			List<DatabaseSynchronizeFromCrm> databaseSynchronizeFromCrmList = DatabaseOptionBase.ReadAllowed<DatabaseSynchronizeFromCrm>(connection);

			return databaseSynchronizeFromCrmList.Select(databaseSynchronizeFromCrm => new SynchronizeFromCrm(connection, databaseSynchronizeFromCrm)).ToList();
		}

		protected override bool ExecuteOption()
		{
			Guid changeProviderId = _databaseSynchronizeFromCrm.changeProviderId;
			string urlLoginName = _databaseSynchronizeFromCrm.urlLoginName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection connection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			DateTime currentSearchDate = DateTime.Now;

			DataLayer.MongoData.Progress progress;
			DateTime searchDate = GetSearchDate(out progress);

			List<Contact> contacts = Contact.ReadLatest(connection, searchDate);

			contacts.ForEach(contact => StoreInContactChangesIfNeeded(contact, changeProviderId));

			progress.UpdateAndSetLastProgressDateToNow(Connection);

			return true;
		}

		internal void StoreInContactChangesIfNeeded(Contact crmContact, Guid changeProviderId)
		{
			Guid externalContactId = crmContact.ContactId;
			DataLayer.SqlData.Contact.ExternalContact externalContact = DataLayer.SqlData.Contact.ExternalContact.ReadOrCreate(SqlConnection, externalContactId, changeProviderId);

			DatabaseContact contact = ReadOrCreateContact(crmContact, externalContactId);

			Guid contactId = contact.Id;
			DateTime modifiedOn = crmContact.ModifiedOn;

			bool ContactChangeExists = DatabaseContactChange.ContactChangeExists(SqlConnection, contactId, externalContactId, changeProviderId, modifiedOn);

			if (ContactChangeExists == true)
			{
				return;
			}

			CreateContactChange(changeProviderId, crmContact, externalContactId, contactId, modifiedOn);
		}

		private DatabaseContact ReadOrCreateContact(Contact crmContact, Guid externalContactId)
		{
			DatabaseContact contact = ContactCrmMapping.FindContact(Connection, SqlConnection, externalContactId, crmContact);

			if (contact == null)
			{
				contact = CreateContact(SqlConnection, crmContact);
			}

			return contact;
		}

		private DatabaseContact CreateContact(SqlConnection sqlConnection, Contact crmContact)
		{
			DatabaseContact contact = new DatabaseContact()
			{
				CreatedOn = DateTime.Now,
				ModifiedOn = crmContact.ModifiedOn,
				Firstname = crmContact.Firstname,
				Lastname = crmContact.Lastname,
			};

			contact.Insert(sqlConnection);

			return contact;
		}

		private void CreateContactChange(Guid changeProviderId, Contact crmContact, Guid externalContactId, Guid contactId, DateTime modifiedOn)
		{
			DatabaseContactChange contactChange = new DatabaseContactChange(SqlConnection, contactId, externalContactId, changeProviderId);

			contactChange.CreatedOn = crmContact.CreatedOn;
			contactChange.ModifiedOn = crmContact.ModifiedOn;
			contactChange.Firstname = crmContact.Firstname;
			contactChange.Lastname = crmContact.Lastname;

			contactChange.Insert();
		}

		private DateTime GetSearchDate(out DataLayer.MongoData.Progress progress)
		{
			string progressName = "DynamicsCrmFrom";

			progress = DataLayer.MongoData.Progress.ReadNext(Connection, progressName);

			if (progress == null)
			{
				progress = new DataLayer.MongoData.Progress()
				{
					LastProgressDate = DateTime.MinValue,
					TargetId = Guid.Empty,
					TargetName = progressName,
				};

				progress.Insert(Connection);
			}

			DateTime searchDate = progress.LastProgressDate;

			if (searchDate <= DateTime.MinValue.AddHours(1))
			{
				return DateTime.MinValue;
			}

			return progress.LastProgressDate.AddHours(-1);
		}
	}
}
