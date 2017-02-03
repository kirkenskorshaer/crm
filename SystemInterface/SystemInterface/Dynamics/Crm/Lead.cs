using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SystemInterface.Dynamics.Crm
{
	public class Lead : AbstractCrm
	{
		public string address1_city;
		public string address1_line1;
		public string address1_line2;
		public string address1_line3;
		public string address1_name;
		public string address1_postalcode;
		public string address1_telephone1;
		public string address1_telephone2;
		public string emailaddress1;
		public string emailaddress2;
		public string emailaddress3;
		public string firstname;
		public string jobtitle;
		public string lastname;
		public string middlename;
		public string mobilephone;
		public string subject;
		public string telephone1;
		public string telephone2;
		public string telephone3;
		public string new_oprindelse;
		public string new_oprindelseip;
		public int? new_mailrelaysubscriberid;

		public EntityReference campaignid;
		public Guid? campaign { get { return GetEntityReferenceId(campaignid); } set { campaignid = SetEntityReferenceId(value, "campaign"); } }

		public EntityReference parentcontactid;
		public Guid? parentcontact { get { return GetEntityReferenceId(parentcontactid); } set { parentcontactid = SetEntityReferenceId(value, "contact"); } }

		public EntityReference new_indsamler2016;
		public Guid? indsamler2016 { get { return GetEntityReferenceId(new_indsamler2016); } set { new_indsamler2016 = SetEntityReferenceId(value, "account"); } }

		private static readonly ColumnSet ColumnSetLead = new ColumnSet(
			"leadid",
			"address1_city",
			"address1_line1",
			"address1_line2",
			"address1_line3",
			"address1_name",
			"address1_postalcode",
			"address1_telephone1",
			"address1_telephone2",
			"campaignid",
			"emailaddress1",
			"emailaddress2",
			"emailaddress3",
			"firstname",
			"jobtitle",
			"lastname",
			"middlename",
			"mobilephone",
			"owningbusinessunit",
			"subject",
			"telephone1",
			"telephone2",
			"telephone3",
			"new_oprindelse",
			"new_oprindelseip",
			"new_mailrelaysubscriberid",
			"new_indsamler2016");

		private static readonly ColumnSet ColumnSetLeadCrmGenerated = new ColumnSet("createdon", "modifiedon");

		public Lead(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public Lead(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetLeadCrmGenerated; } }

		protected override string entityName { get { return "lead"; } }
		protected override string idName { get { return "leadid"; } }

		protected override Entity GetAsEntity(bool includeContactId)
		{
			Entity crmEntity = new Entity("lead");

			if (includeContactId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_city", address1_city));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line1", address1_line1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line2", address1_line2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line3", address1_line3));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_name", address1_name));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_postalcode", address1_postalcode));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_telephone1", address1_telephone1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_telephone2", address1_telephone2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("campaignid", campaignid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("parentcontactid", parentcontactid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("emailaddress1", emailaddress1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("emailaddress2", emailaddress2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("emailaddress3", emailaddress3));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("firstname", firstname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("jobtitle", jobtitle));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("lastname", lastname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("middlename", middlename));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("mobilephone", mobilephone));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("owningbusinessunit", owningbusinessunit));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("subject", subject));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("telephone1", telephone1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("telephone2", telephone2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("telephone3", telephone3));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_oprindelse", new_oprindelse));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_oprindelseip", new_oprindelseip));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_mailrelaysubscriberid", new_mailrelaysubscriberid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_indsamler2016", new_indsamler2016));

			return crmEntity;
		}

		public static void UpdateSubscriberId(IDynamicsCrmConnection dynamicsCrmConnection, Guid leadId, int subscriberId)
		{
			AbstractCrm.Update(dynamicsCrmConnection, "lead", "leadid", leadId, new Dictionary<string, object>()
			{
				{ "new_mailrelaysubscriberid", subscriberId },
			});
		}

		public static List<Lead> ReadAllLeadIds(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "leadid" }, new Dictionary<string, string>(), null, (connection, entity) => new Lead(connection, entity), new PagingInformation());
		}

		public static List<Lead> ReadFromFetchXml(IDynamicsCrmConnection dynamicsCrmConnection, List<string> fields, Dictionary<string, string> keyContent)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, fields, keyContent, null, (connection, contactEntity) => new Lead(connection, contactEntity), new PagingInformation());
		}

		public static Lead Create(IDynamicsCrmConnection dynamicsCrmConnection, Dictionary<string, string> allContent)
		{
			Lead lead = new Lead(dynamicsCrmConnection);
			CreateFromContent(dynamicsCrmConnection, lead, allContent);

			return lead;
		}
	}
}
