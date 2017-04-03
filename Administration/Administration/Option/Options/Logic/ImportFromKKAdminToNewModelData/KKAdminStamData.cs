using System;
using System.Collections.Generic;
using System.Linq;

namespace Administration.Option.Options.Logic.ImportFromKKAdminToNewModelData
{
	public class KKAdminStamData : AbstractKKAdminData
	{
		public string Navn { get; set; }
		//public string CprNr { get; set; }
		public string GaveBrevUdløbsDato { get; set; }
		public DateTime? GaveBrevUdløbsDatoDateTime { get { return GetParsedDateTime(GaveBrevUdløbsDato); } }
		public string MobilNr { get; set; }
		public string HarGaveBrev { get; set; }
		public string FødtDato { get; set; }
		public DateTime? FødtDatoDateTime { get { return GetParsedDateTime(FødtDato); } }
		public string KredsBladAntal { get; set; }
		public string KorsBladAntal { get; set; }
		public string ByNavn { get; set; }
		public string PostNr { get; set; }
		public string StorKredsNavn { get; set; }
		public string StorKredsNr { get; set; }
		public string Notat { get; set; }
		public string Email { get; set; }
		public string Telefon { get; set; }
		public string Adresse2 { get; set; }
		public string Adresse1 { get; set; }
		public string Titel { get; set; }
		public string Aktiv { get; set; }
		public string RowId { get; set; }
		public string MedlemsNr { get; set; }

		public List<KKAdminIndbetaling> Indbetalinger;
		public List<KKAdminTilknytning> Tilknytninger;

		public DataTypeEnum DataType
		{
			get
			{
				if (Tilknytninger.Any(tilknytning => tilknytning.TilknytningsNavn == "Arbejdsgrenes adresser"))
				{
					return DataTypeEnum.Account;
				}
				return DataTypeEnum.Contact;
			}
		}

		public enum DataTypeEnum
		{
			Account = 1,
			Contact = 2,
		}

		internal bool IsValidIndbetaling(DateTime firstValidIndbetaling)
		{
			return Indbetalinger.Any(indbetaling =>	indbetaling.IndbetalingDatoDateTime.GetValueOrDefault(DateTime.MinValue) >= firstValidIndbetaling);
		}

		internal void SetRelations(List<KKAdminTilknytning> tilknytninger, List<KKAdminIndbetaling> indbetalinger)
		{
			Indbetalinger = indbetalinger.Where(indbetaling => indbetaling.MedlemsNr == MedlemsNr).ToList();

			Tilknytninger = tilknytninger.Where(tilknytning => tilknytning.MedlemsNr == MedlemsNr).ToList();
		}

		public Dictionary<string, string> ToCrmContact()
		{
			string[] nameSplit = Navn.Split(new char[] { ' ' }, 2);
			string firstname = nameSplit.First();
			string lastname = string.Empty;

			if (nameSplit.Length == 2)
			{
				lastname = nameSplit.Last();
			}

			Dictionary<string, string> crmContact = new Dictionary<string, string>();

			AddValueIfFilled(crmContact, "firstname", firstname);
			AddValueIfFilled(crmContact, "middlename", null);
			AddValueIfFilled(crmContact, "lastname", lastname);
			AddValueIfFilled(crmContact, "address1_line1", Adresse1);
			AddValueIfFilled(crmContact, "address1_line2", Adresse2);
			AddValueIfFilled(crmContact, "new_titel", Titel);
			AddValueIfFilled(crmContact, "new_kkadminstatus", Aktiv);
			AddValueIfFilled(crmContact, "new_kkadminmedlemsnr", MedlemsNr);
			//AddValueIfFilled(crmContact, "new_cprnr", CprNr);
			AddValueIfFilled(crmContact, "address1_city", ByNavn);
			AddValueIfFilled(crmContact, "address1_postalcode", PostNr);
			AddValueIfFilled(crmContact, "emailaddress1", Email);
			AddValueIfFilled(crmContact, "mobilephone", Telefon);
			AddValueIfFilled(crmContact, "telephone1", Telefon);
			AddValueIfFilled(crmContact, "birthdate", FødtDatoDateTime?.ToString(DateTimeCrmFormat));
			AddValueIfFilled(crmContact, "new_gavebrevudloebsdato", GaveBrevUdløbsDatoDateTime?.ToString(DateTimeCrmFormat));
			AddValueIfFilled(crmContact, "new_storkredsnavn", StorKredsNavn);
			AddValueIfFilled(crmContact, "new_storkredsnr", StorKredsNr);
			AddValueIfFilled(crmContact, "new_kkadminsoegenavn", null);
			AddValueIfFilled(crmContact, "new_hargavebrev", HarGaveBrev);

			return crmContact;
		}

		internal Dictionary<string, string> ToCrmAccount()
		{
			Dictionary<string, string> crmAccount = new Dictionary<string, string>();

			AddValueIfFilled(crmAccount, "name", Navn);
			AddValueIfFilled(crmAccount, "address1_line1", Adresse1);
			AddValueIfFilled(crmAccount, "address1_line2", Adresse2);
			AddValueIfFilled(crmAccount, "new_kkadminstatus", Aktiv);
			AddValueIfFilled(crmAccount, "new_kkadminmedlemsnr", MedlemsNr);
			AddValueIfFilled(crmAccount, "telephone1", Telefon);
			AddValueIfFilled(crmAccount, "address1_city", ByNavn);
			AddValueIfFilled(crmAccount, "address1_postalcode", PostNr);
			AddValueIfFilled(crmAccount, "emailaddress1", Email);

			return crmAccount;
		}
	}
}
