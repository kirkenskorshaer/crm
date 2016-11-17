using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Utilities.Converter;

namespace Administration.Option.Options.Logic.ImportDanskeBankData
{
	public class Iso20022Document
	{
		public string IBAN { get; private set; }
		public decimal TtlNetNtryAmt { get; private set; }

		public List<Iso20022Ntry> Ntries = new List<Iso20022Ntry>();

		private XNamespace _namespace;
		public Iso20022Document(XDocument bankXml)
		{
			_namespace = bankXml.Root.GetDefaultNamespace();

			IEnumerable<XElement> Ntrys;

			if (_namespace.NamespaceName == "urn:iso:std:iso:20022:tech:xsd:camt.053.001.02")
			{
				XElement Stmt = bankXml.Element(_namespace + "Document").Element(_namespace + "BkToCstmrStmt").Element(_namespace + "Stmt");
				IBAN = Stmt.Element(_namespace + "Acct").Element(_namespace + "Id").Element(_namespace + "IBAN").Value;

				Ntrys = Stmt.Elements(_namespace + "Ntry");

				TtlNetNtryAmt = decimal.Parse(Stmt.Element(_namespace + "TxsSummry").Element(_namespace + "TtlNtries").Element(_namespace + "TtlNetNtryAmt").Value);
			}
			else
			{
				XElement Ntfctn = bankXml.Element(_namespace + "Document").Element(_namespace + "BkToCstmrDbtCdtNtfctn").Element(_namespace + "Ntfctn");
				IBAN = Ntfctn.Element(_namespace + "Acct").Element(_namespace + "Id").Element(_namespace + "IBAN").Value;

				Ntrys = Ntfctn.Elements(_namespace + "Ntry");

				TtlNetNtryAmt = decimal.Parse(Ntfctn.Element(_namespace + "TxsSummry").Element(_namespace + "TtlNtries").Element(_namespace + "TtlNetNtryAmt").Value);
			}

			foreach (XElement Ntry in Ntrys)
			{
				string bankId = Ntry.Element(_namespace + "AddtlInfInd")?.Element(_namespace + "MsgNmId")?.Value;
				if (string.IsNullOrWhiteSpace(bankId))
				{
					XElement NtryRef = Ntry.Element(_namespace + "NtryRef");
					if (NtryRef != null)
					{
						NtryRef.Remove();
					}

					bankId = Md5Helper.MakeMd5(Ntry.ToString());
				}

				Iso20022Ntry ntry = new Iso20022Ntry()
				{
					BankId = bankId,
					ValDt = XmlConvert.ToDateTime(Ntry.Element(_namespace + "ValDt").Element(_namespace + "Dt").Value, XmlDateTimeSerializationMode.Unspecified),
					Amt = decimal.Parse(Ntry.Element(_namespace + "Amt").Value, new CultureInfo("en-US")),
					Ccy = Ntry.Element(_namespace + "Amt").Attribute("Ccy").Value,
					Prtry = Ntry.Element(_namespace + "NtryDtls")?.Element(_namespace + "TxDtls")?.Element(_namespace + "Purp")?.Element(_namespace + "Prtry")?.Value,
					BkTxCdDomnCd = Ntry.Element(_namespace + "BkTxCd")?.Element(_namespace + "Domn")?.Element(_namespace + "Cd")?.Value,
					BkTxCdDomnFmlyCd = Ntry.Element(_namespace + "BkTxCd")?.Element(_namespace + "Domn")?.Element(_namespace + "Fmly")?.Element(_namespace + "Cd")?.Value,
					BkTxCdDomnFmlySubFmlyCd = Ntry.Element(_namespace + "BkTxCd")?.Element(_namespace + "Domn")?.Element(_namespace + "Fmly")?.Element(_namespace + "SubFmlyCd")?.Value,
					BkTxCdPrtryCd = Ntry.Element(_namespace + "BkTxCd")?.Element(_namespace + "Prtry")?.Element(_namespace + "Cd")?.Value,
				};

				Ntries.Add(ntry);
			}
		}
	}
}
