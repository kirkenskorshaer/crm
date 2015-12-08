using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.DsiNext
{
	public class Contact
	{
		public string new_cprnr { get; private set; }

		public string address1_line1 { get; private set; }
		public string address1_line2 { get; private set; }
		public string address1_city { get; private set; }
		public string address1_postalcode { get; private set; }

		public string emailaddress1 { get; private set; }

		public string mobilephone { get; private set; }
		public string telephone1 { get; private set; }

		public string firstname { get; private set; }
		public string lastname { get; private set; }

		public int new_RowId { get; private set; }
		public int new_KKAdminMedlemsNr { get; private set; }

		public DateTime? birthdate { get; private set; }
		public DateTime? new_GaveBrevUdloebsDato { get; private set; }
		public bool new_HarGaveBrev { get; private set; }

		public string Notat { get; private set; }

		public string createdby { get; private set; }
		public DateTime createdon { get; private set; }
		public StatusEnum new_KKAdminStatus { get; private set; }
		public string new_StorKredsNavn { get; private set; }
		public int new_StorKredsNr { get; private set; }
		public string new_KKAdminSoegeNavn { get; private set; }
		public List<Group> Groups { get; private set; }
		public string new_Titel { get; private set; }
		public string modifiedby { get; private set; }
		public DateTime modifiedon { get; private set; }

		public static List<Contact> Read(DateTime lastChange, int pageNumber, int pageSize)
		{
			KKAdminService.KKAdminServiceClient client = new KKAdminService.KKAdminServiceClient();
			KKAdminService.Stamdata[] stamDataArray = client.GetSomeStamdataByDateWithPaging(lastChange, pageNumber, pageSize);

			List<Contact> contacts = stamDataArray.Select(ToContact).ToList();

			return contacts;
		}

		private static Contact ToContact(KKAdminService.Stamdata stamData)
		{
			Contact contact = new Contact
			{
				address1_line1 = stamData.Adresse1,
				address1_line2 = stamData.Adresse2,
				address1_city = stamData.ByNavn,
				new_cprnr = stamData.CprNr,
				emailaddress1 = stamData.Email,
				birthdate = stamData.FødtDato,
				new_GaveBrevUdloebsDato = stamData.GaveBrevUdløbsDato,
				new_HarGaveBrev = stamData.HarGaveBrev,
				new_KKAdminMedlemsNr = stamData.MedlemsNr,
				mobilephone = stamData.MobilNr,

				Notat = stamData.Notat,

				createdby = stamData.OprettetAf,
				createdon = stamData.OprettetDato,
				address1_postalcode = stamData.PostNr,
				new_RowId = stamData.RowId,
				new_KKAdminStatus = GetStatus(stamData.Status),
				new_StorKredsNavn = stamData.StorKredsNavn,
				new_StorKredsNr = stamData.StorKredsNr,
				new_KKAdminSoegeNavn = stamData.SøgeNavn,
				telephone1 = stamData.Telefon,

				Groups = stamData.Tilknytning.Select(tilknytning => new Group()
				{
					Name = tilknytning.Navn,
				}).ToList(),

				new_Titel = stamData.Titel,
				modifiedby = stamData.ÆndretAf,
				modifiedon = stamData.ÆndretDato,
			};

			SetName(contact, stamData.Navn);

			return contact;
		}

		public enum StatusEnum
		{
			InActive = 0,
			Active = 1,
		}

		private static StatusEnum GetStatus(KKAdminService.StatusTypes kkAdminType)
		{
			switch (kkAdminType)
			{
				case KKAdminService.StatusTypes.InActive:
					return StatusEnum.InActive;
				case KKAdminService.StatusTypes.Active:
					return StatusEnum.Active;
				default:
					throw new Exception($"Unknown kkAdminType {kkAdminType}");
			}
		}

		private static void SetName(Contact contact, string navn)
		{
			if (string.IsNullOrWhiteSpace(navn))
			{
				return;
			}

			string[] nameParts = navn.Split(new char[] { ' ' }, 2);

			contact.firstname = nameParts[0];

			if (nameParts.Length == 1)
			{
				return;
			}

			contact.lastname = nameParts[1];
		}
	}
}
