using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace SystemInterface.Dynamics.Crm
{
	public class Campaign : AbstractCrm
	{
		public string name;
		public DateTime createdon { get; private set; }
		public DateTime modifiedon { get; private set; }

		private static readonly ColumnSet ColumnSetCampaign = new ColumnSet(
			"name",
			"campaignid",

			"createdon",
			"modifiedon",
			"modifiedby"
		);

		private static readonly ColumnSet ColumnSetCampaignCrmGenerated = new ColumnSet("createdon", "modifiedon", "modifiedby");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetCampaignCrmGenerated; } }

		protected override string entityName { get { return "campaign"; } }
		protected override string idName { get { return "campaignid"; } }

		public Campaign(DynamicsCrmConnection dynamicsCrmConnection) : base(dynamicsCrmConnection)
		{
		}

		public Campaign(DynamicsCrmConnection dynamicsCrmConnection, Entity entity) : base(dynamicsCrmConnection, entity)
		{
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("name", name));

			return crmEntity;
		}

		public static Campaign Read(DynamicsCrmConnection dynamicsCrmConnection, Guid campaignid)
		{
			Entity campaignEntity = dynamicsCrmConnection.Service.Retrieve("campaign", campaignid, ColumnSetCampaign);

			Campaign campaign = new Campaign(dynamicsCrmConnection, campaignEntity);

			return campaign;
		}

		private static readonly DateTime _minimumSearchDate = new DateTime(1900, 1, 1);
		public static List<Campaign> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfCampaigns = null)
		{
			List<Campaign> campaigns = StaticCrm.ReadLatest(connection, "campaign", ColumnSetCampaign, lastSearchDate, (lConnection, entity) => new Campaign(lConnection, entity), maximumNumberOfCampaigns);

			return campaigns;
		}
	}
}
