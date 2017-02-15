using System;
using System.Collections.Generic;
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
		public string new_leadtarget;
		public int? new_mailrelaygroupid;

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

		protected override Entity GetAsEntity(bool includeId)
		{
			Entity crmEntity = new Entity("campaign");

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("name", name));

			return crmEntity;
		}

		public void SetLeadtarget(string newUrl)
		{
			Update(Connection, entityName, idName, Id, new Dictionary<string, object>(){
				{ "new_leadtarget", newUrl }
			});
		}

		public static List<Campaign> ReadAllCampaignIds(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "campaignid" }, new Dictionary<string, string>(), null, (connection, entity) => new Campaign(connection, entity), new PagingInformation());
		}

		public static List<Campaign> ReadAllCampaignsWithIdAndLeadtarget(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "campaignid", "new_leadtarget" }, new Dictionary<string, string>(), null, (connection, entity) => new Campaign(connection, entity), new PagingInformation());
		}

		public static List<Campaign> ReadCampaignsToImportStubDataTo(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "campaignid", "new_redirecttarget", "new_collecttype", "new_mailrelaygroupid", "name", "ownerid" }, new Dictionary<string, string>(), null, (connection, entity) => new Campaign(connection, entity), new PagingInformation());
		}

		public List<string> GetKeyFields()
		{
			List<Field> fields = ReadNNRelationship(_keyFieldsRelationshipName, GetAsIdEntity(), (entity) => new Field(Connection, entity), "new_name");

			return fields.Select(field => field.new_name).ToList();
		}

		public static void WriteIndbetalingsum(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			WriteMoneyByName(dynamicsCrmConnection, id, amount, "new_indbetalingsum");
		}

		public static void WriteIndbetalingsumBy(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			WriteMoneyByName(dynamicsCrmConnection, id, amount, "new_indbetalingsumby");
		}

		public static void WriteIndbetalingsumKreds(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			WriteMoneyByName(dynamicsCrmConnection, id, amount, "new_indbetalingsumkreds");
		}

		public static void WriteOptaltsumBy(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			WriteMoneyByName(dynamicsCrmConnection, id, amount, "new_optaltsumby");
		}

		public static void WriteOptaltsumKreds(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			WriteMoneyByName(dynamicsCrmConnection, id, amount, "new_optaltsumkreds");
		}

		public static void WriteOptaltsum(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			WriteMoneyByName(dynamicsCrmConnection, id, amount, "new_optaltsum");
		}

		private static void WriteMoneyByName(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount, string name)
		{
			Update(dynamicsCrmConnection, "campaign", "campaignid", id, new Dictionary<string, object>()
			{
				{ name, new Money(amount) }
			});
		}

		public static void WriteIndbetalingSums
		(
			IDynamicsCrmConnection dynamicsCrmConnection, Guid campaignid,
			decimal indbetalingsum, decimal indbetalingsumBy, decimal indbetalingsumKreds,
			decimal? indbetalingsumBankoverfoersel, decimal? indbetalingsumGiro, decimal? indbetalingsumKontant, decimal? indbetalingsumMobilePay, decimal? indbetalingsumSms, decimal? indbetalingsumSwipp, decimal? indbetalingsumUkendt
		)
		{
			Update(dynamicsCrmConnection, "campaign", "campaignid", campaignid, new Dictionary<string, object>()
			{
				{ "new_indbetalingsum", new Money(indbetalingsum) },
				{ "new_indbetalingsumby", new Money(indbetalingsumBy) },
				{ "new_indbetalingsumkreds", new Money(indbetalingsumKreds) },
				{ "new_indbetalingsumbankoverfoersel", new Money(indbetalingsumBankoverfoersel.GetValueOrDefault(0)) },
				{ "new_indbetalingsumgiro", new Money(indbetalingsumGiro.GetValueOrDefault(0)) },
				{ "new_indbetalingsummobilepay", new Money(indbetalingsumMobilePay.GetValueOrDefault(0)) },
				{ "new_indbetalingsumsms", new Money(indbetalingsumSms.GetValueOrDefault(0)) },
				{ "new_indbetalingsumkontant", new Money(indbetalingsumKontant.GetValueOrDefault(0)) },
				{ "new_indbetalingsumswipp", new Money(indbetalingsumSwipp.GetValueOrDefault(0)) },
				{ "new_indbetalingsumukendt", new Money(indbetalingsumUkendt.GetValueOrDefault(0)) },
			});
		}
	}
}
