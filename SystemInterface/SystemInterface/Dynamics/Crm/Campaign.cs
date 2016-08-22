using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Campaign : AbstractCrm
	{
		public string name;
		public string new_redirecttarget;
		public DateTime createdon;
		public DateTime modifiedon;

		private string _keyFieldsRelationshipName = "new_campaign_field_key";

		public OptionSetValue new_collecttype;
		public collecttypeEnum? collecttype { get { return GetOptionSet<collecttypeEnum>(new_collecttype); } set { new_collecttype = SetOptionSet((int?)value); } }

		private static readonly ColumnSet ColumnSetCampaign = new ColumnSet(
			"campaignid",
			"name",
			"new_collecttype",
			"new_redirecttarget");

		private static readonly ColumnSet ColumnSetCampaignCrmGenerated = new ColumnSet("createdon", "modifiedon");

		public Campaign(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public Campaign(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetCampaignCrmGenerated; } }

		protected override string entityName { get { return "campaign"; } }
		protected override string idName { get { return "campaignid"; } }

		public enum collecttypeEnum
		{
			Lead = 100000000,
			ContactOgLeadVedEksisterendeContact = 100000001,
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity("campaign");

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("name", name));

			return crmEntity;
		}

		public static List<Campaign> ReadAllCampaignIds(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "campaignid" }, new Dictionary<string, string>(), null, (connection, entity) => new Campaign(connection, entity), new PagingInformation());
		}

		public static List<Campaign> ReadCampaignsToImportStubDataTo(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "campaignid", "new_redirecttarget", "new_collecttype", "name", "ownerid" }, new Dictionary<string, string>(), null, (connection, entity) => new Campaign(connection, entity), new PagingInformation());
		}

		public List<string> GetKeyFields()
		{
			List<Field> fields = ReadNNRelationship(_keyFieldsRelationshipName, GetAsIdEntity(), (entity) => new Field(Connection, entity));

			return fields.Select(field => field.new_name).ToList();
		}
	}
}
