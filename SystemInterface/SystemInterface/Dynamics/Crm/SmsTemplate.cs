using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class SmsTemplate : AbstractValueEntity
	{
		public string new_text;
		public string new_fetchxml;

		public SmsTemplate(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity, "new_smstemplate", "new_smstemplateid")
		{
		}

		public static SmsTemplate GetWaitingTemplate(IDynamicsCrmConnection _dynamicsCrmConnection)
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch", new XAttribute("count", 1),
					new XElement("entity", new XAttribute("name", "new_smstemplate"),
						new XElement("attribute", new XAttribute("name", "new_text")),
						new XElement("attribute", new XAttribute("name", "new_smstemplateid")),
						new XElement("attribute", new XAttribute("name", "new_fetchxml")),
						new XElement("link-entity", new XAttribute("name", "new_sms"), new XAttribute("from", "new_smstemplateid"), new XAttribute("to", "new_smstemplateid"), new XAttribute("link-type", "inner"),
							new XElement("filter", new XAttribute("type", "and"),
								new XElement("condition", new XAttribute("attribute", "new_direction"), new XAttribute("operator", "eq"), new XAttribute("value", "0")),
								new XElement("condition", new XAttribute("attribute", "new_sendstatus"), new XAttribute("operator", "eq"), new XAttribute("value", "100000000")),
								new XElement("filter", new XAttribute("type", "or"),
									new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "null")),
									new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "le"), new XAttribute("value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
								)
							),
							new XElement("link-entity", new XAttribute("name", "contact"), new XAttribute("from", "contactid"), new XAttribute("to", "new_contactid"), new XAttribute("link-type", "inner"),
								new XElement("filter", new XAttribute("type", "and"),
									new XElement("condition", new XAttribute("attribute", "mobilephone"), new XAttribute("operator", "not-null"))
								)
							)
						)
					)
				)
			);

			SmsTemplate template = StaticCrm.ReadFromFetchXml(_dynamicsCrmConnection, xDocument, (dynamicsCrmConnection, entity) => new SmsTemplate(dynamicsCrmConnection, entity)).SingleOrDefault();

			return template;
		}

		public IDictionary<string, object> GetFields(Guid contactId)
		{
			if (string.IsNullOrWhiteSpace(new_fetchxml))
			{
				return new Dictionary<string, object>();
			}

			XDocument xDocument = XDocument.Parse(new_fetchxml);

			XmlHelper.AddCondition(xDocument, "contactid", "eq", contactId.ToString());

			List<dynamic> receivers = DynamicFetch.ReadFromFetchXml(Connection, xDocument, new PagingInformation());

			if (receivers == null)
			{
				return new Dictionary<string, object>();
			}

			if (receivers.Count != 1)
			{
				throw new Exception($"sms template error for template {Id}");
			}

			IDictionary<string, object> receiverDictionary = (IDictionary<string, object>)receivers.Single();

			return receiverDictionary;
		}
	}
}
