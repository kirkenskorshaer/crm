using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Utilities.Converter;

namespace SystemInterface.Dynamics.Crm
{
	public class MailrelayInformation : AbstractCrm
	{
		public string fullname;
		public string firstname;
		public string middlename;
		public string lastname;
		public string address1_line1;
		public string address1_line2;
		public string address1_line3;
		public string address1_postalcode;
		public string address1_city;
		public string mobilephone;
		public DateTime createdon;
		public int? campaign_new_mailrelaygroupid;
		public string emailaddress1;

		public string Indsamlingssted2016_emailaddress1;
		public string Indsamlingssted2016_address1_composite;
		public string Indsamlingssted2016_name;
		public int? Indsamlingssted2016_new_kkadminmedlemsnr;
		public string Indsamlingssted2016_new_rutpassword;
		public string Indsamlingssted2016_address1_line1;
		public string Indsamlingssted2016_address1_line2;
		public string Indsamlingssted2016_address1_line3;
		public string Indsamlingssted2016_address1_postalcode;
		public string Indsamlingssted2016_address1_city;
		public string Indsamlingssted2016_new_rutbrugernavn;

		public string Indsamlingskoordinator_fullname;
		public string Indsamlingskoordinator_emailaddress1;
		public string Indsamlingskoordinator_mobilephone;
		public string Indsamlingskoordinator_firstname;
		public string Indsamlingskoordinator_middlename;
		public string Indsamlingskoordinator_lastname;
		public string Indsamlingskoordinator_telephone1;

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

		public void UpdateContactMailrelaycheck(DynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, "contact", "contactid", contactid.Value, new Dictionary<string, object>()
			{
				{ "new_mailrelaycheck", new_mailrelaycheck }
			});
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

			AddIfValueNotEmpty(customFields, "f_11", firstname);
			AddIfValueNotEmpty(customFields, "f_12", middlename);
			AddIfValueNotEmpty(customFields, "f_13", lastname);
			AddIfValueNotEmpty(customFields, "f_14", address1_line1);
			AddIfValueNotEmpty(customFields, "f_15", address1_line2);
			AddIfValueNotEmpty(customFields, "f_16", address1_line3);
			AddIfValueNotEmpty(customFields, "f_17", address1_postalcode);
			AddIfValueNotEmpty(customFields, "f_18", address1_city);
			AddIfValueNotEmpty(customFields, "f_19", mobilephone);

			AddIfValueNotEmpty(customFields, "f_3", Indsamlingssted2016_emailaddress1);
			AddIfValueNotEmpty(customFields, "f_4", Indsamlingssted2016_address1_composite);
			AddIfValueNotEmpty(customFields, "f_1", Indsamlingssted2016_name);
			AddIfValueNotEmpty(customFields, "f_20", Indsamlingssted2016_new_kkadminmedlemsnr);
			AddIfValueNotEmpty(customFields, "f_21", Indsamlingssted2016_new_rutpassword);
			AddIfValueNotEmpty(customFields, "f_22", Indsamlingssted2016_address1_line1);
			AddIfValueNotEmpty(customFields, "f_23", Indsamlingssted2016_address1_line2);
			AddIfValueNotEmpty(customFields, "f_24", Indsamlingssted2016_address1_line3);
			AddIfValueNotEmpty(customFields, "f_25", Indsamlingssted2016_address1_postalcode);
			AddIfValueNotEmpty(customFields, "f_26", Indsamlingssted2016_address1_city);
			AddIfValueNotEmpty(customFields, "f_32", Indsamlingssted2016_new_rutbrugernavn);

			AddIfValueNotEmpty(customFields, "f_5", Indsamlingskoordinator_fullname);
			AddIfValueNotEmpty(customFields, "f_6", Indsamlingskoordinator_emailaddress1);
			AddIfValueNotEmpty(customFields, "f_7", Indsamlingskoordinator_mobilephone);
			AddIfValueNotEmpty(customFields, "f_27", Indsamlingskoordinator_firstname);
			AddIfValueNotEmpty(customFields, "f_28", Indsamlingskoordinator_middlename);
			AddIfValueNotEmpty(customFields, "f_29", Indsamlingskoordinator_lastname);
			AddIfValueNotEmpty(customFields, "f_30", Indsamlingskoordinator_telephone1);

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

		private void AddIfValueNotEmpty(Dictionary<string, string> customFields, string key, int? value)
		{
			if (value.HasValue == false)
			{
				return;
			}

			customFields.Add(key, value.ToString());
		}

		public bool RecalculateContactCheck()
		{
			string newCheck = string.Empty;

			newCheck = Xor.XorString(newCheck, fullname);
			newCheck = Xor.XorString(newCheck, firstname);
			newCheck = Xor.XorString(newCheck, middlename);
			newCheck = Xor.XorString(newCheck, lastname);
			newCheck = Xor.XorString(newCheck, address1_line1);
			newCheck = Xor.XorString(newCheck, address1_line2);
			newCheck = Xor.XorString(newCheck, address1_line3);
			newCheck = Xor.XorString(newCheck, address1_postalcode);
			newCheck = Xor.XorString(newCheck, address1_city);
			newCheck = Xor.XorString(newCheck, mobilephone);
			newCheck = Xor.XorString(newCheck, emailaddress1);

			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_emailaddress1);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_address1_composite);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_name);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_new_kkadminmedlemsnr.GetValueOrDefault(int.MinValue).ToString());
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_new_rutpassword);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_address1_line1);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_address1_line2);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_address1_line3);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_address1_postalcode);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_address1_city);
			newCheck = Xor.XorString(newCheck, Indsamlingssted2016_new_rutbrugernavn);

			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_fullname);
			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_emailaddress1);
			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_mobilephone);
			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_firstname);
			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_middlename);
			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_lastname);
			newCheck = Xor.XorString(newCheck, Indsamlingskoordinator_telephone1);

			newCheck = Md5Helper.MakeMd5(newCheck);

			if (new_mailrelaycheck == newCheck)
			{
				return false;
			}

			new_mailrelaycheck = newCheck;
			return true;
		}
	}
}
