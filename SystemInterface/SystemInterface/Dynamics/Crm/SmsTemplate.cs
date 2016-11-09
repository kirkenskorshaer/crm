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
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", "new_smstemplate"), new XAttribute("count", 1),
						new XElement("attribute", new XAttribute("name", "new_text")),
						new XElement("attribute", new XAttribute("name", "new_smstemplateid")),
						new XElement("attribute", new XAttribute("name", "new_fetchxml")),
						new XElement("link-entity", new XAttribute("name", "new_sms"), new XAttribute("from", "new_smstemplateid"), new XAttribute("to", "new_smstemplateid"), new XAttribute("link-type", "inner"),
							new XElement("filter", new XAttribute("type", "and"),
								new XElement("condition", new XAttribute("attribute", "new_actualsenttime"), new XAttribute("operator", "null")),
								new XElement("filter", new XAttribute("type", "or"),
									new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "null")),
									new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "le"), new XAttribute("value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
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
