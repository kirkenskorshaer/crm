using DataLayer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemInterface.Dynamics.Crm;
using DatabaseSetMissingCampaignLeadtarget = DataLayer.MongoData.Option.Options.Logic.SetMissingCampaignLeadtarget;

namespace Administration.Option.Options.Logic
{
	public class SetMissingCampaignLeadtarget : OptionBase
	{
		private DatabaseSetMissingCampaignLeadtarget _databaseSetMissingCampaignLeadtarget;

		public SetMissingCampaignLeadtarget(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSetMissingCampaignLeadtarget = (DatabaseSetMissingCampaignLeadtarget)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseSetMissingCampaignLeadtarget.urlLoginName;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			string baseUrl = _databaseSetMissingCampaignLeadtarget.baseUrl;

			List<Campaign> campaigns = Campaign.ReadAllCampaignsWithIdAndLeadtarget(_dynamicsCrmConnection);
			foreach (Campaign campaign in campaigns)
			{
				string oldUrl = campaign.new_leadtarget;

				StringBuilder urlBuilder = new StringBuilder();

				urlBuilder.Append($"{baseUrl}?formId={campaign.Id}");

				List<string> keyFields = campaign.GetKeyFields();
				foreach (string field in keyFields)
				{
					urlBuilder.Append($"&{field}=[{field}]");
				}

				string newUrl = urlBuilder.ToString();

				if (oldUrl != newUrl)
				{
					campaign.SetLeadtarget(newUrl);
				}
			}

			report.Success = true;
		}

		public static List<SetMissingCampaignLeadtarget> Find(MongoConnection connection)
		{
			List<DatabaseSetMissingCampaignLeadtarget> options = DatabaseSetMissingCampaignLeadtarget.ReadAllowed<DatabaseSetMissingCampaignLeadtarget>(connection);

			return options.Select(option => new SetMissingCampaignLeadtarget(connection, option)).ToList();
		}
	}
}
