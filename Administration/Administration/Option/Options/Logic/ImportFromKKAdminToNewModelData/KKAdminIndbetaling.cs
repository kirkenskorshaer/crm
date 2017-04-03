using System;
using System.Collections.Generic;
using Utilities.Converter;

namespace Administration.Option.Options.Logic.ImportFromKKAdminToNewModelData
{
	public class KKAdminIndbetaling : AbstractKKAdminData
	{
		public string MedlemsNr { get; set; }
		public string Navn { get; set; }
		public string IndbetalingStatus { get; set; }
		public string TakkeBrev { get; set; }
		public string IndbetalingsTypeNavn { get; set; }
		public string IndbetalingTekst { get; set; }
		public string IndbetalingDato { get; set; }
		public DateTime? IndbetalingDatoDateTime { get { return GetParsedDateTime(IndbetalingDato); } }
		public string KontoType { get; set; }
		public string FinansKonto { get; set; }
		public string BogføringsDato { get; set; }
		public DateTime? BogføringsDatoDateTime { get { return GetParsedDateTime(BogføringsDato); } }
		public string Beløb { get; set; }
		public Decimal BeløbDecimal { get { return Decimal.Parse(Beløb.Replace(',', '.')); } }

		public string BankId
		{
			get
			{
				return Md5Helper.MakeMd5(ToString());
			}
		}

		public KKAdminIndbetaling()
		{
			DateTimeCsvFormat = "yyyy-MM-dd";
		}

		public override string ToString()
		{
			return
				"MedlemsNr: " + MedlemsNr
				+ "Navn: " + Navn
				+ "IndbetalingStatus: " + IndbetalingStatus
				+ "TakkeBrev: " + TakkeBrev
				+ "IndbetalingsTypeNavn: " + IndbetalingsTypeNavn
				+ "IndbetalingTekst: " + IndbetalingTekst
				+ "IndbetalingDato: " + IndbetalingDato
				+ "KontoType: " + KontoType
				+ "FinansKonto: " + FinansKonto
				+ "BogføringsDato: " + BogføringsDato
				+ "Beløb: " + Beløb;
		}
	}
}
