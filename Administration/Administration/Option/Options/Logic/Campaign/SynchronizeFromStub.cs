using Administration.Option.Options.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSynchronizeFromStub = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromStub;
using DataLayer.MongoData.Input;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DataLayer;
using Utilities.StaticData;

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

			WebCampaign.CollectTypeEnum collectType = webCampaign.CollectType;

			Stub stub = Stub.ReadFirst(Connection, webCampaign);
			if (stub == null)
			{
				return true;
			}

			try
			{
				SynchronizeStub(stub, keyField, changeProviderId, collectType);
				stub.Delete(Connection);
				return true;
			}
			catch (Exception exception)
			{
				Log.Write(Connection, exception.Message, exception.StackTrace, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				stub.ImportAttempt++;
				stub.Update(Connection);
				return false;
			}
		}

		private DatabaseChangeProvider GetChangeProvider(Guid CampaignId)
		{
			string name = $"WebCampaign {CampaignId}";

			DatabaseChangeProvider changeProvider = DatabaseChangeProvider.ReadByNameOrCreate(SqlConnection, name);

			return changeProvider;
		}

		private void SynchronizeStub(Stub stub, string keyField, Guid changeProviderId, WebCampaign.CollectTypeEnum collectType)
		{
			string keyValue = stub.Contents.Single(content => content.Key == keyField).Value;
			Guid keyId = Utilities.Converter.GuidConverter.Convert(keyValue);

			switch (collectType)
			{
				case WebCampaign.CollectTypeEnum.Contact:
					SynchronizeStubContact(stub, keyField, keyValue, keyId, changeProviderId);
					break;
				case WebCampaign.CollectTypeEnum.ContactWithAccountRelation:
					SynchronizeStubContactWithAccountRelation(stub, keyField, keyValue, keyId, changeProviderId);
					break;
				case WebCampaign.CollectTypeEnum.Account:
					SynchronizeStubAccount(stub, keyField, keyValue, keyId, changeProviderId);
					break;
				default:
					break;
			}
		}

		private void SynchronizeStubContact(Stub stub, string keyField, string keyValue, Guid keyId, Guid changeProviderId)
		{
			DatabaseExternalContact externalContact = ReadOrCreateExternalContact(stub, keyField, keyValue, keyId, changeProviderId);

			DatabaseContactChange contactChange = new DatabaseContactChange(SqlConnection, externalContact.ContactId, externalContact.ExternalContactId, changeProviderId);

			Conversion.Contact.Convert(stub, contactChange);
			contactChange.createdon = stub.PostTime;
			contactChange.modifiedon = stub.PostTime;

			contactChange.Insert();
		}

		private void SynchronizeStubAccount(Stub stub, string keyField, string keyValue, Guid keyId, Guid changeProviderId)
		{
			DatabaseExternalAccount externalAccount = ReadOrCreateExternalAccount(stub, keyField, keyValue, keyId, changeProviderId);

			DatabaseAccountChange accountChange = new DatabaseAccountChange(SqlConnection, externalAccount.AccountId, externalAccount.ExternalAccountId, changeProviderId);

			Conversion.Account.Convert(stub, accountChange);
			accountChange.createdon = stub.PostTime;
			accountChange.modifiedon = stub.PostTime;

			accountChange.Insert();
		}

		private void SynchronizeStubContactWithAccountRelation(Stub stub, string keyField, string keyValue, Guid keyId, Guid changeProviderId)
		{
			DatabaseExternalContact externalContact = ReadOrCreateExternalContact(stub, keyField, keyValue, keyId, changeProviderId);

			DatabaseAccountChange accountChange = null;
			if (stub.Contents.Any(content => content.Key == ImportRelationshipNames.indsamlingssted2016))
			{
				accountChange = GetAccountChangeForRelation(stub, keyField, keyId, ImportRelationshipNames.indsamlingssted2016, changeProviderId);
			}

			DatabaseContactChange contactChange = new DatabaseContactChange(SqlConnection, externalContact.ContactId, externalContact.ExternalContactId, changeProviderId);
			if (accountChange == null)
			{
				Conversion.Contact.Convert(stub, contactChange);
			}
			else
			{
				Conversion.Contact.Convert(stub, contactChange, accountChange, SqlConnection);
			}
			contactChange.createdon = stub.PostTime;
			contactChange.modifiedon = stub.PostTime;

			contactChange.Insert();
		}

		private DatabaseAccountChange GetAccountChangeForRelation(Stub stub, string keyField, Guid keyId, string relationshipName, Guid changeProviderId)
		{
			string relationshipValue = stub.Contents.Single(content => content.Key == relationshipName).Value;
			Guid relationshipId = Guid.Parse(relationshipValue);

			DatabaseExternalAccount externalAccount = DatabaseExternalAccount.ReadOrCreate(SqlConnection, keyId, changeProviderId, relationshipId);
			DatabaseAccountChange accountChange = new DatabaseAccountChange(SqlConnection, externalAccount.AccountId, externalAccount.ExternalAccountId, changeProviderId);
			accountChange.createdon = stub.PostTime;
			accountChange.modifiedon = stub.PostTime;
			accountChange.Insert();
			return accountChange;
		}

		private DatabaseExternalContact ReadOrCreateExternalContact(Stub stub, string keyField, string keyValue, Guid keyId, Guid changeProviderId)
		{
			bool externalContactExists = DatabaseExternalContact.Exists(SqlConnection, keyId, changeProviderId);

			DatabaseContact contact = null;
			DatabaseExternalContact externalContact = null;

			if (externalContactExists)
			{
				externalContact = DatabaseExternalContact.Read(SqlConnection, keyId, changeProviderId);
				contact = DatabaseContact.Read(SqlConnection, externalContact.ContactId);
			}
			else
			{
				contact = ReadOrCreateContact(stub, keyField, keyValue);
				externalContact = new DatabaseExternalContact(SqlConnection, keyId, changeProviderId, contact.Id);
				externalContact.Insert();
			}

			return externalContact;
		}

		private DatabaseContact ReadOrCreateContact(Stub stub, string keyField, string keyValue)
		{
			Guid? contactId = DatabaseContact.ReadIdFromField(SqlConnection, keyField, keyValue);
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

		private DatabaseExternalAccount ReadOrCreateExternalAccount(Stub stub, string keyField, string keyValue, Guid keyId, Guid changeProviderId)
		{
			bool externalAccountExists = DatabaseExternalAccount.Exists(SqlConnection, keyId, changeProviderId);

			DatabaseAccount account = null;
			DatabaseExternalAccount externalAccount = null;

			if (externalAccountExists)
			{
				externalAccount = DatabaseExternalAccount.Read(SqlConnection, keyId, changeProviderId);
				account = DatabaseAccount.Read(SqlConnection, externalAccount.AccountId);
			}
			else
			{
				account = ReadOrCreateAccount(stub, keyField, keyValue);
				externalAccount = new DatabaseExternalAccount(SqlConnection, keyId, changeProviderId, account.Id);
				externalAccount.Insert();
			}

			return externalAccount;
		}

		private DatabaseAccount ReadOrCreateAccount(Stub stub, string keyField, string value)
		{
			Guid? accountId = DatabaseAccount.ReadIdFromField(SqlConnection, keyField, value);
			DatabaseAccount account;

			if (accountId.HasValue)
			{
				account = DatabaseAccount.Read(SqlConnection, accountId.Value);
			}
			else
			{
				account = new DatabaseAccount()
				{
					createdon = stub.PostTime,
					modifiedon = stub.PostTime,
				};

				account.Insert(SqlConnection);
			}

			return account;
		}
	}
}
