using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Sms : AbstractValueEntity
	{
		public string mobilephone;
		public DateTime? new_sendtime;
		public DateTime? new_operatorsendtime;

		public EntityReference new_contactid;
		public Guid? contactid { get { return GetEntityReferenceId(new_contactid); } set { new_contactid = SetEntityReferenceId(value, "contact"); } }

		public Sms(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity, "new_sms", "new_smsid")
		{
		}

		public static IEnumerable<Sms> GetWaitingSmsOnTemplate(IDynamicsCrmConnection _dynamicsCrmConnection, Guid templateId)
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", "new_sms"),
						new XElement("attribute", new XAttribute("name", "new_smsid")),
						new XElement("attribute", new XAttribute("name", "new_sendtime")),
						new XElement("attribute", new XAttribute("name", "new_operatorsendtime")),
						new XElement("attribute", new XAttribute("name", "new_contactid")),
						new XElement("filter", new XAttribute("type", "and"),
							new XElement("condition", new XAttribute("attribute", "new_direction"), new XAttribute("operator", "eq"), new XAttribute("value", "0")),
							new XElement("condition", new XAttribute("attribute", "new_sendstatus"), new XAttribute("operator", "eq"), new XAttribute("value", "100000000")),
							new XElement("condition", new XAttribute("attribute", "new_smstemplateid"), new XAttribute("operator", "eq"), new XAttribute("value", templateId)),
							new XElement("filter", new XAttribute("type", "or"),
								new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "null")),
								new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "le"), new XAttribute("value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
							)
						),
						new XElement("link-entity", new XAttribute("name", "contact"), new XAttribute("from", "contactid"), new XAttribute("to", "new_contactid"), new XAttribute("link-type", "inner"),
							new XElement("attribute", new XAttribute("name", "mobilephone"), new XAttribute("alias", "mobilephone")),
							new XElement("filter", new XAttribute("type", "and"),
								new XElement("condition", new XAttribute("attribute", "mobilephone"), new XAttribute("operator", "not-null"))
							)
						)
					)
				)
			);

			IEnumerable<Sms> smsCollection = StaticCrm.ReadFromFetchXml(_dynamicsCrmConnection, xDocument, (dynamicsCrmConnection, entity) => new Sms(dynamicsCrmConnection, entity));

			return smsCollection;
		}

		public void MarkAsSent(string text, string externalId)
		{
			Update(Connection, entityName, idName, Id, new Dictionary<string, object>()
			{
				{ "new_text", text },
				{ "new_externalid", externalId },
				{ "new_sendtime", new_sendtime },
				{ "new_operatorsendtime", new_operatorsendtime },
				{ "new_sendstatus", new OptionSetValue(100000002) },
			});
		}

		public void UpdateStatus(string operatorstatus)
		{
			Update(Connection, entityName, idName, Id, new Dictionary<string, object>()
			{
				{ "new_operatorstatus", operatorstatus },
			});
		}
	}
}
