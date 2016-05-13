using Administration.Option.Options.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSynchronizeFromStub = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromStub;
using DataLayer.MongoData.Input;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DataLayer;
using DataLayer.SqlData.Contact;
using System.Data.SqlClient;

namespace Administration.Option.Options.Logic.Campaign
{
	public class SynchronizeFromStub : AbstractDataOptionBase
	{
		private DatabaseSynchronizeFromStub _databaseSynchronizeFromStub;

		public SynchronizeFromStub(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeFromStub = (DatabaseSynchronizeFromStub)databaseOption;
		}

		public static List<SynchronizeFromStub> Find(MongoConnection connection)
		{
			List<DatabaseSynchronizeFromStub> databaseSynchronizeFromStubList = DatabaseOptionBase.ReadAllowed<DatabaseSynchronizeFromStub>(connection);

			return databaseSynchronizeFromStubList.Select(databaseSynchronizeFromStub => new SynchronizeFromStub(connection, databaseSynchronizeFromStub)).ToList();
		}

		protected override bool ExecuteOption()
		{
			WebCampaign webCampaign = WebCampaign.ReadByIdBytesSingleOrDefault(Connection, _databaseSynchronizeFromStub.WebCampaignIdValue());

			string keyField = webCampaign.KeyField;
			Guid formId = webCampaign.FormId;

			Guid changeProviderId = GetChangeProvider(webCampaign.FormId).Id;

			Stub stub = Stub.ReadFirst(Connection, webCampaign);
			if (stub == null)
			{
				return true;
			}

			SynchronizeStub(stub, keyField, changeProviderId);

			stub.Delete(Connection);

			return true;
		}

		private DatabaseChangeProvider GetChangeProvider(Guid CampaignId)
		{
			string name = $"WebCampaign {CampaignId}";

			DatabaseChangeProvider changeProvider = DatabaseChangeProvider.ReadByNameOrCreate(SqlConnection, name);

			return changeProvider;
		}

		private void SynchronizeStub(Stub stub, string keyField, Guid changeProviderId)
		{
			DatabaseExternalContact externalContact = ReadOrCreateExternalContact(stub, keyField, changeProviderId);

			DatabaseContactChange contactChange = new DatabaseContactChange(SqlConnection, externalContact.ContactId, externalContact.ExternalContactId, changeProviderId);

			Conversion.Contact.Convert(stub, contactChange);
			contactChange.createdon = stub.PostTime;
			contactChange.modifiedon = stub.PostTime;

			contactChange.Insert();
		}

		private DatabaseExternalContact ReadOrCreateExternalContact(Stub stub, string keyField, Guid changeProviderId)
		{
			string value = stub.Contents.Single(content => content.Key == keyField).Value;

			Guid externalContactId = Utilities.Converter.GuidConverter.Convert(value);

			bool externalContactExists = DatabaseExternalContact.Exists(SqlConnection, externalContactId, changeProviderId);

			DatabaseContact contact = null;
			DatabaseExternalContact externalContact = null;

			if (externalContactExists)
			{
				externalContact = DatabaseExternalContact.Read(SqlConnection, externalContactId, changeProviderId);
				contact = DatabaseContact.Read(SqlConnection, externalContact.ContactId);
			}
			else
			{
				contact = ReadOrCreateContact(stub, keyField, value);
				externalContact = new DatabaseExternalContact(SqlConnection, externalContactId, changeProviderId, contact.Id);
				externalContact.Insert();
			}

			return externalContact;
		}

		private DatabaseContact ReadOrCreateContact(Stub stub, string keyField, string value)
		{
			Guid? contactId = DatabaseContact.ReadIdFromField(SqlConnection, keyField, value);
			DatabaseContact contact;

			if (contactId.HasValue)
			{
				contact = DatabaseContact.Read(SqlConnection, contactId.Value);
			}
			else
			{
				contact = new DatabaseContact()
				{
					createdon = stub.PostTime,
					modifiedon = stub.PostTime,
				};

				contact.Insert(SqlConnection);
			}

			return contact;
		}
	}
}
