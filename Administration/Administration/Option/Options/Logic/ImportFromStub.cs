﻿using System.Collections.Generic;
using System.Linq;
using DataLayer;
using DatabaseImportFromStub = DataLayer.MongoData.Option.Options.Logic.ImportFromStub;
using DatabaseStub = DataLayer.MongoData.Input.Stub;
using DatabaseStubElement = DataLayer.MongoData.Input.StubElement;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using System;
using SystemInterface.Dynamics.Crm;
using DataLayer.MongoData;

namespace Administration.Option.Options.Logic
{
	public class ImportFromStub : OptionBase
	{
		private DatabaseImportFromStub _databaseImportFromStub;

		public ImportFromStub(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseImportFromStub = (DatabaseImportFromStub)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseImportFromStub.urlLoginName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadByIdBytesSingleOrDefault(Connection, _databaseImportFromStub.WebCampaignIdValue());

			DatabaseWebCampaign.CollectTypeEnum collectType;
			DatabaseStub stub;

			if (webCampaign == null)
			{
				collectType = DatabaseWebCampaign.CollectTypeEnum.Lead;
				stub = DatabaseStub.ReadFirst(Connection);
			}
			else
			{
				collectType = webCampaign.CollectType;
				stub = DatabaseStub.ReadFirst(Connection, webCampaign);
			}

			if (stub == null)
			{
				report.Success = true;
				return;
			}

			try
			{
				ImportStub(dynamicsCrmConnection, stub, webCampaign, collectType);
				stub.Delete(Connection);
				report.Success = true;
				return;
			}
			catch (Exception exception)
			{
				Log.Write(Connection, exception.Message, typeof(ImportFromStub), exception.StackTrace, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				stub.ImportAttempt++;
				stub.Update(Connection);
				report.Success = false;
				return;
			}
		}

		private void ImportStub(DynamicsCrmConnection dynamicsCrmConnection, DatabaseStub stub, DatabaseWebCampaign webCampaign, DatabaseWebCampaign.CollectTypeEnum collectType)
		{
			List<string> keyFields;

			if (webCampaign == null)
			{
				keyFields = new List<string>();
			}
			else
			{
				keyFields = webCampaign.KeyFields;
			}

			List<DatabaseStubElement> contentList = Utilities.LinqExtension.DistinctBy(stub.Contents, content => content.Key).ToList();

			contentList = contentList.Where(content => content.Key != null).ToList();

			Dictionary<string, string> allContent = contentList.ToDictionary(content => content.Key, content => content.Value);

			Dictionary<string, string> keyContent = allContent.Where(content => keyFields.Contains(content.Key)).ToDictionary(content => content.Key, content => content.Value);

			Contact contact = null;

			int? numberOfMatchingContacts = null;
			if (keyContent.Any())
			{
				List<Contact> matchingContacts = Contact.ReadFromFetchXml(dynamicsCrmConnection, new List<string> { "contactid", "ownerid" }, keyContent);
				numberOfMatchingContacts = matchingContacts.Count;

				if (numberOfMatchingContacts == 1)
				{
					contact = matchingContacts.Single();
				}
			}

			Lead lead = Lead.Create(dynamicsCrmConnection, allContent);

			Guid? owner = null;

			if (webCampaign != null)
			{
				lead.campaign = webCampaign.FormId;
				owner = webCampaign.FormOwner;
			}

			if (contact != null && owner.HasValue == false)
			{
				if (owner.HasValue == false)
				{
					owner = contact.owner;
				}
			}

			switch (collectType)
			{
				case DatabaseWebCampaign.CollectTypeEnum.LeadOgContactHvisContactIkkeFindes:
					if (contact == null && numberOfMatchingContacts == 0)
					{
						contact = Contact.Create(dynamicsCrmConnection, allContent);
						contact.InsertWithoutRead();
						if (owner.HasValue)
						{
							contact.owner = owner;
							contact.Assign();
						}
					}
					break;
			}

			lead.subject = _databaseImportFromStub.Name;

			if (contact != null)
			{
				lead.parentcontact = contact.Id;
			}

			lead.InsertWithoutRead();

			if (owner.HasValue)
			{
				lead.owner = owner;
				lead.Assign();
			}

			if (webCampaign != null && webCampaign.mailrelaygroupid.HasValue)
			{
				AddMailrelaySubscriberFromLead.CreateIfValid(Connection, lead.Id, $"Auto subscribe from {_databaseImportFromStub.Name}", _databaseImportFromStub.urlLoginName, lead.emailaddress1, webCampaign);
			}
		}
	}
}