using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Indbetaling : AbstractValueEntity
	{
		public string new_bankid;
		public Money new_amount;
		public string new_name;
		public DateTime? new_valdt;

		public EntityReference new_byarbejdeid;
		public Guid? byarbejdeid { get { return GetEntityReferenceId(new_byarbejdeid); } set { new_byarbejdeid = SetEntityReferenceId(value, "new_byarbejde"); } }

		public EntityReference new_campaignid;
		public Guid? campaignid { get { return GetEntityReferenceId(new_campaignid); } set { new_campaignid = SetEntityReferenceId(value, "campaign"); } }

		public EntityReference new_indsamlingskoordinatorid;
		public Guid? indsamlingskoordinatorid { get { return GetEntityReferenceId(new_indsamlingskoordinatorid); } set { new_indsamlingskoordinatorid = SetEntityReferenceId(value, "contact"); } }

		public EntityReference new_indsamlingsstedid;
		public Guid? indsamlingsstedid { get { return GetEntityReferenceId(new_indsamlingsstedid); } set { new_indsamlingsstedid = SetEntityReferenceId(value, "account"); } }

		public EntityReference new_kontoid;
		public Guid? kontoid { get { return GetEntityReferenceId(new_kontoid); } set { new_kontoid = SetEntityReferenceId(value, "new_konto"); } }

		public OptionSetValue new_kilde;
		public kildeEnum? kilde { get { return GetOptionSet<kildeEnum>(new_kilde); } set { new_kilde = SetOptionSet((int?)value); } }

		public void SetMoney(decimal amount)
		{
			new_amount = new Money(amount);
		}

		public decimal? GetMoney()
		{
			return new_amount?.Value;
		}

		public enum kildeEnum
		{
			Kontant = 100000000,
			MobilePay = 100000001,
			Sms = 100000002,
			Swipp = 100000003,
			Giro = 100000004,
			Bankoverfoersel = 100000005,
			Ukendt = 100000100,
		}

		public Indbetaling(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity, "new_indbetaling", "new_indbetalingid")
		{
		}

		private Indbetaling(IDynamicsCrmConnection connection) : base(connection, "new_indbetaling", "new_indbetalingid")
		{
		}

		public static IEnumerable<Indbetaling> GetIndbetalingOnIban(IDynamicsCrmConnection dynamicsCrmConnection, string iban)
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", "new_indbetaling"),
						new XElement("attribute", new XAttribute("name", "new_amount")),
						new XElement("attribute", new XAttribute("name", "new_bankid")),
						new XElement("attribute", new XAttribute("name", "new_byarbejdeid")),
						new XElement("attribute", new XAttribute("name", "new_campaignid")),
						new XElement("attribute", new XAttribute("name", "new_indsamlingskoordinatorid")),
						new XElement("attribute", new XAttribute("name", "new_indsamlingsstedid")),
						new XElement("attribute", new XAttribute("name", "new_kilde")),
						new XElement("attribute", new XAttribute("name", "new_kontoid")),
						new XElement("link-entity", new XAttribute("name", "new_konto"), new XAttribute("from", "new_kontoid"), new XAttribute("to", "new_kontoid"), new XAttribute("link-type", "inner"),
							new XElement("filter", new XAttribute("type", "and"),
								new XElement("condition", new XAttribute("attribute", "new_iban"), new XAttribute("operator", "eq"), new XAttribute("value", iban))
							)
						)
					)
				)
			);

			IEnumerable<Indbetaling> indbetalingCollection = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (lDynamicsCrmConnection, entity) => new Indbetaling(lDynamicsCrmConnection, entity));

			return indbetalingCollection;
		}

		public static Indbetaling CreateAndInsert(IDynamicsCrmConnection dynamicsCrmConnection, string iban, decimal amt, string bankid, string prtry, DateTime valDt, Guid kontoId, Guid campaignId, kildeEnum kilde, Guid? byarbejdeid, Guid? indsamlingsstedid, Guid? indsamlingskoordinatorid, Guid owner)
		{
			Indbetaling indbetaling = new Indbetaling(dynamicsCrmConnection);

			indbetaling.byarbejdeid = byarbejdeid;
			indbetaling.campaignid = campaignId;
			indbetaling.indsamlingskoordinatorid = indsamlingskoordinatorid;
			indbetaling.indsamlingsstedid = indsamlingsstedid;
			indbetaling.kilde = kilde;
			indbetaling.SetMoney(amt);
			indbetaling.kontoid = kontoId;
			indbetaling.new_bankid = bankid;
			indbetaling.new_valdt = valDt;
			indbetaling.new_name = "Auto oprettet indbetaling";
			indbetaling.InsertWithoutRead();

			indbetaling.owner = owner;

			indbetaling.Assign();

			return indbetaling;
		}
	}
}
