using System;
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

		public MailrelayInformation(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public MailrelayInformation(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		public static MailrelayInformation GetMailrelayFromLead(IDynamicsCrmConnection dynamicsCrmConnection, Func<string, string> getResourcePath, string email, Guid campaignId)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Mailrelay/GetMailrelayFromLead.xml");

			XDocument xDocument = XDocument.Load(path);

			xDocument.Element("fetch").Element("entity").Element("filter").Element("condition").Attribute("value").Value = email;

			xDocument.Element("fetch").Element("entity").Elements("link-entity").Single(element => element.Attribute("name").Value == "campaign").Element("filter").Element("condition").Attribute("value").Value = campaignId.ToString();

			List<MailrelayInformation> informations = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (connection, entity) => new MailrelayInformation(connection, entity), new PagingInformation());

			MailrelayInformation information = informations.OrderByDescending(lInformation => lInformation.createdon).FirstOrDefault();

			return information;
		}

		public void UpdateContactMailrelaycheck(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, "contact", "contactid", contactid.Value, new Dictionary<string, object>()
			{
				{ "new_mailrelaycheck", new_mailrelaycheck }
			});
		}

		public void UpdateContactMailrelaySubscriberid(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, "contact", "contactid", contactid.Value, new Dictionary<string, object>()
			{
				{ "new_mailrelaysubscriberid", new_mailrelaysubscriberid }
			});
		}

		public static MailrelayInformation GetInformationNotInMailrelayFromContact(IDynamicsCrmConnection dynamicsCrmConnection, Func<string, string> getResourcePath, Guid contactId)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Mailrelay/GetMailrelayFromContact.xml");

			XDocument xDocument = XDocument.Load(path);

			XmlHelper.AddCondition(xDocument, "contactid", "eq", contactId.ToString());

			List<MailrelayInformation> informations = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (connection, entity) => new MailrelayInformation(connection, entity), new PagingInformation());

			return informations.Single();
		}

		public static List<MailrelayInformation> GetMailrelayFromContact(IDynamicsCrmConnection dynamicsCrmConnection, Func<string, string> getResourcePath, PagingInformation pagingInformation, int pageSize, Guid? contactId)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/Mailrelay/GetMailrelayFromContact.xml");

			XDocument xDocument = XDocument.Load(path);

			xDocument.Element("fetch").Attribute("count").Value = pageSize.ToString();

			if (contactId.HasValue)
			{
				XmlHelper.AddCondition(xDocument, "contactid", "eq", contactId.Value.ToString());
			}

			XmlHelper.AddCondition(xDocument, "new_mailrelaysubscriberid", "not-null");

			List<MailrelayInformation> informations = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (connection, entity) => new MailrelayInformation(connection, entity), pagingInformation);

			return informations;
		}

		protected override Entity GetAsEntity(bool includeId)
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, string> GetCustomFields()
		{
			Dictionary<string, string> customFields = new Dictionary<string, string>();

			AddValue(customFields, "f_11", firstname);
			AddValue(customFields, "f_12", middlename);
			AddValue(customFields, "f_13", lastname);
			AddValue(customFields, "f_14", address1_line1);
			AddValue(customFields, "f_15", address1_line2);
			AddValue(customFields, "f_16", address1_line3);
			AddValue(customFields, "f_17", address1_postalcode);
			AddValue(customFields, "f_18", address1_city);
			AddValue(customFields, "f_19", mobilephone);

			AddValue(customFields, "f_3", Indsamlingssted2016_emailaddress1);
			AddValue(customFields, "f_4", Indsamlingssted2016_address1_composite);
			AddValue(customFields, "f_1", Indsamlingssted2016_name);
			AddIfValueNotEmpty(customFields, "f_20", Indsamlingssted2016_new_kkadminmedlemsnr);
			AddValue(customFields, "f_21", Indsamlingssted2016_new_rutpassword);
			AddValue(customFields, "f_22", Indsamlingssted2016_address1_line1);
			AddValue(customFields, "f_23", Indsamlingssted2016_address1_line2);
			AddValue(customFields, "f_24", Indsamlingssted2016_address1_line3);
			AddValue(customFields, "f_25", Indsamlingssted2016_address1_postalcode);
			AddValue(customFields, "f_26", Indsamlingssted2016_address1_city);
			AddValue(customFields, "f_32", Indsamlingssted2016_new_rutbrugernavn);

			AddValue(customFields, "f_5", Indsamlingskoordinator_fullname);
			AddValue(customFields, "f_6", Indsamlingskoordinator_emailaddress1);
			AddValue(customFields, "f_7", Indsamlingskoordinator_mobilephone);
			AddValue(customFields, "f_27", Indsamlingskoordinator_firstname);
			AddValue(customFields, "f_28", Indsamlingskoordinator_middlename);
			AddValue(customFields, "f_29", Indsamlingskoordinator_lastname);
			AddValue(customFields, "f_30", Indsamlingskoordinator_telephone1);

			return customFields;
		}

		private void AddValue(Dictionary<string, string> customFields, string key, string value)
		{
			if (value == null)
			{
				customFields.Add(key, string.Empty);
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
