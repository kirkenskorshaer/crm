using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Konto : AbstractValueEntity
	{
		public string name;
		public string iban;

		public EntityReference new_campaignid;
		public Guid? campaignid { get { return GetEntityReferenceId(new_campaignid); } set { new_campaignid = SetEntityReferenceId(value, "campaign"); } }

		public Konto(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity, "new_konto", "new_kontoid")
		{
		}

		private Konto(IDynamicsCrmConnection connection) : base(connection, "new_indbetaling", "new_indbetalingid")
		{
		}

		public static Konto ReadFromIban(IDynamicsCrmConnection dynamicsCrmConnection, string iban)
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", "new_konto"),
						new XElement("attribute", new XAttribute("name", "new_kontoid")),
						new XElement("attribute", new XAttribute("name", "new_iban")),
						new XElement("attribute", new XAttribute("name", "new_campaignid")),
						new XElement("attribute", new XAttribute("name", "ownerid")),
						new XElement("filter", new XAttribute("type", "and"),
							new XElement("condition", new XAttribute("attribute", "new_iban"), new XAttribute("operator", "eq"), new XAttribute("value", iban))
						)
					)
				)
			);

			IEnumerable<Konto> kontoCollection = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (lDynamicsCrmConnection, entity) => new Konto(lDynamicsCrmConnection, entity));

			return kontoCollection.SingleOrDefault();
		}

		public static Konto CreateAndInsert(IDynamicsCrmConnection dynamicsCrmConnection, string iban)
		{
			Konto konto = new Konto(dynamicsCrmConnection);

			konto.name = $"auto oprettet {DateTime.Now.ToString("yyyy-MM-dd")}";
			konto.iban = iban;

			konto.InsertWithoutRead();

			return konto;
		}

		public static void WriteIndbetalingsum(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			Update(dynamicsCrmConnection, "new_konto", "new_kontoid", id, new Dictionary<string, object>()
			{
				{ "new_indbetalingsum", new Money(amount) }
			});
		}
	}
}
