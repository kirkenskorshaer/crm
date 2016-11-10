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

		public EntityReference new_toid;
		public Guid? toid { get { return GetEntityReferenceId(new_toid); } set { new_toid = SetEntityReferenceId(value, "contact"); } }

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
						new XElement("attribute", new XAttribute("name", "new_toid")),
						new XElement("link-entity", new XAttribute("name", "new_sms"), new XAttribute("from", "new_smstemplateid"), new XAttribute("to", "new_smstemplateid"), new XAttribute("link-type", "inner"),
							new XElement("filter", new XAttribute("type", "and"),
								new XElement("condition", new XAttribute("attribute", "new_actualsenttime"), new XAttribute("operator", "null")),
								new_direction
								new XElement("filter", new XAttribute("type", "or"),
									new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "null")),
									new XElement("condition", new XAttribute("attribute", "new_sendtime"), new XAttribute("operator", "le"), new XAttribute("value", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
								)
							)
						),
						new XElement("link-entity", new XAttribute("name", "contact"), new XAttribute("from", "contactid"), new XAttribute("to", "new_toid"), new XAttribute("link-type", "inner"),
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
				{ "new_sendtime", DateTime.Now },
				{ "new_externalid", externalId },
			});
		}
	}
}
