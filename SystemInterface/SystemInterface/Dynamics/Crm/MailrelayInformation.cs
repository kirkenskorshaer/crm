using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class MailrelayInformation : AbstractCrm
	{
		public string fullname;
		public DateTime createdon;
		public int? campaign_new_mailrelaygroupid;
		public string emailaddress1;
		public string Indsamlingssted2016_emailaddress1;
		public string Indsamlingssted2016_address1_composite;
		public string Indsamlingssted2016_name;
		public string Indsamlingskoordinator_fullname;
		public string Indsamlingskoordinator_emailaddress1;
		public string Indsamlingskoordinator_mobilephone;
		public Guid? contactid;
		public int? new_mailrelaysubscriberid;
		public string new_mailrelaycheck;

		protected override string entityName { get { return "lead"; } }

		protected override string idName { get { return "leadid"; } }

		protected override ColumnSet ColumnSetCrmGenerated { get { throw new NotImplementedException(); } }

		public MailrelayInformation(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public MailrelayInformation(DynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		public static MailrelayInformation GetMailrelayFromLead(DynamicsCrmConnection dynamicsCrmConnection, Func<string, string> getResourcePath, string email, Guid campaignId)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Mailrelay/GetMailrelayFromLead.xml");

			XDocument xDocument = XDocument.Load(path);

			xDocument.Element("fetch").Element("entity").Element("filter").Element("condition").Attribute("value").Value = email;

			xDocument.Element("fetch").Element("entity").Elements("link-entity").Single(element => element.Attribute("name").Value == "campaign").Element("filter").Element("condition").Attribute("value").Value = campaignId.ToString();

			List<MailrelayInformation> informations = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (connection, entity) => new MailrelayInformation(connection, entity), new PagingInformation());

			MailrelayInformation information = informations.OrderByDescending(lInformation => lInformation.createdon).FirstOrDefault();

			return information;
		}

		public static List<MailrelayInformation> GetMailrelayFromContact(DynamicsCrmConnection dynamicsCrmConnection, Func<string, string> getResourcePath, PagingInformation pagingInformation, int pageSize, Guid? contactId)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Mailrelay/GetMailrelayFromContact.xml");

			XDocument xDocument = XDocument.Load(path);

			xDocument.Element("fetch").Attribute("count").Value = pageSize.ToString();

			if (contactId.HasValue)
			{
				xDocument.Element("fetch").Element("entity").Element("filter").Add(new XElement("condition",
					new XAttribute("attribute", "contactid"),
					new XAttribute("operator", "eq"),
					new XAttribute("value", contactId.Value)));
			}

			List<MailrelayInformation> informations = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (connection, entity) => new MailrelayInformation(connection, entity), pagingInformation);

			return informations;
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, string> GetCustomFields()
		{
			Dictionary<string, string> customFields = new Dictionary<string, string>();

			AddIfValueNotEmpty(customFields, "f_1", Indsamlingssted2016_name);
			AddIfValueNotEmpty(customFields, "f_3", Indsamlingssted2016_emailaddress1);
			AddIfValueNotEmpty(customFields, "f_4", Indsamlingssted2016_address1_composite);
			AddIfValueNotEmpty(customFields, "f_5", Indsamlingskoordinator_fullname);
			AddIfValueNotEmpty(customFields, "f_6", Indsamlingskoordinator_emailaddress1);
			AddIfValueNotEmpty(customFields, "f_7", Indsamlingskoordinator_mobilephone);

			return customFields;
		}

		private void AddIfValueNotEmpty(Dictionary<string, string> customFields, string key, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			customFields.Add(key, value);
		}
	}
}
