using System.Collections.Generic;
using System.Linq;
using DatabaseCreateImportFromStub = DataLayer.MongoData.Option.Options.Logic.CreateImportFromStub;
using DatabaseImportFromStub = DataLayer.MongoData.Option.Options.Logic.ImportFromStub;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseSchedule = DataLayer.MongoData.Option.Schedule;
using SystemInterface.Dynamics.Crm;
using DataLayer;
using System;

namespace Administration.Option.Options.Logic
{
	public class CreateImportFromStub : OptionBase
	{
		private DatabaseCreateImportFromStub _databaseCreateImportFromStub;

		public CreateImportFromStub(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseCreateImportFromStub = (DatabaseCreateImportFromStub)databaseOption;
		}

		protected override bool ExecuteOption()
		{
			string urlLoginName = _databaseCreateImportFromStub.urlLoginName;
			DatabaseSchedule ImportFromStubSchedule = _databaseCreateImportFromStub.ImportFromStubSchedule;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			List<Campaign> campaigns = Campaign.ReadCampaignsToImportStubDataTo(dynamicsCrmConnection);

			foreach (Campaign campaign in campaigns)
			{
				DatabaseWebCampaign webCampaign = ReadOrCreateWebCampaign(campaign);

				bool importExists = DatabaseImportFromStub.ReadByWebCampaign(Connection, webCampaign).Any();
				if (importExists == false)
				{
					DatabaseImportFromStub.Create(Connection, webCampaign._id, urlLoginName, campaign.name, ImportFromStubSchedule);
				}
			}

			return true;
		}

		private DatabaseWebCampaign ReadOrCreateWebCampaign(Campaign campaign)
		{
			Guid formId = campaign.Id;
			DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadSingleOrDefault(Connection, formId);

			List<string> keyFields = campaign.GetKeyFields();
			DatabaseWebCampaign newWebCampaign = new DatabaseWebCampaign()
			{
				CollectType = Conversion.Campaign.ToDatabaseEnum(campaign.collecttype),
				FormId = formId,
				FormOwner = campaign.owner.Value,
				KeyFields = keyFields,
				RedirectTarget = campaign.new_redirecttarget,
				mailrelaygroupid = campaign.new_mailrelaygroupid,
			};

			if (webCampaign == null)
			{
				newWebCampaign.Insert(Connection);
				return newWebCampaign;
			}

			if (webCampaign.Equals(newWebCampaign))
			{
				return webCampaign;
			}

			Utilities.ReflectionHelper.Copy(newWebCampaign, webCampaign, new List<string> { "_id", "Id" });
			webCampaign.Update(Connection);

			return webCampaign;
		}

		public static List<CreateImportFromStub> Find(MongoConnection connection)
		{
			List<DatabaseCreateImportFromStub> options = DatabaseCreateImportFromStub.ReadAllowed<DatabaseCreateImportFromStub>(connection);

			return options.Select(option => new CreateImportFromStub(connection, option)).ToList();
		}
	}
}
