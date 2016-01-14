using System;
using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;

namespace Administration.Mapping.RandomData
{
	public class RandomContact : AbstractRandom
	{
		private string[] _lastnames;
		private string[] _firstnames;

		private RandomContact()
		{
			_lastnames = new string[]
			{
				"Jensen",
				"Nielsen",
				"Hansen",
				"Pedersen",
				"Andersen",
				"Christensen",
				"Larsen",
				"Sørensen",
				"Rasmussen",
				"Jørgensen",
				"Petersen",
				"Madsen",
				"Kristensen",
				"Olsen",
				"Thomsen",
				"Christiansen",
				"Poulsen",
				"Johansen",
				"Møller",
				"Mortensen",
			};

			_firstnames = new string[]
			{
				"Anne",
				"Kirsten",
				"Hanne",
				"Mette",
				"Anna",
				"Helle",
				"Susanne",
				"Lene",
				"Maria",
				"Marianne",
				"Inge",
				"Karen",
				"Lone",
				"Bente",
				"Camilla",
				"Pia",
				"Louise",
				"Charlotte",
				"Jette",
				"Tina",

				"Peter",
				"Jens",
				"Lars",
				"Michael",
				"Henrik",
				"Thomas",
				"Søren",
				"Jan",
				"Niels",
				"Christian",
				"Martin",
				"Jørgen",
				"Hans",
				"Anders",
				"Morten",
				"Jesper",
				"Ole",
				"Per",
				"Mads",
				"Erik",
			};
		}

		private static RandomContact Instance;
		public static RandomContact GetInstance()
		{
			if (Instance == null)
			{
				Instance = new RandomContact();
			}

			return Instance;
		}

		public void AnonymizeContact(SystemInterfaceContact contact, int? seed = null)
		{
			Random random;
			if (seed != null)
			{
				random = new Random(seed.Value);
			}
			else
			{
				random = new Random();
			}

			RandomAddress randomAddress = RandomAddress.GetInstance();
			RandomEmail randomEmail = RandomEmail.GetInstance();
			RandomGroup randomGroup = RandomGroup.GetInstance();

			contact.address1_postalcode = randomAddress.GetPostalCode(random);
			contact.address1_city = randomAddress.GetCity(contact.address1_postalcode);
			contact.address1_line1 = GetRandomString(random);
			contact.address1_line2 = GetRandomString(random);
			contact.birthdate = GetRandomDate(random);
			contact.firstname = GetRandomFirstname(random);
			contact.lastname = GetRandomLastName(random);
			contact.emailaddress1 = randomEmail.GetRandomEmail(random, contact.firstname);
			contact.Groups = randomGroup.GetRandomGroups(random);
			contact.mobilephone = GetRandomPhone(random);
			contact.new_cprnr = GetRandomCrp(random);
			contact.new_gavebrevudloebsdato = GetRandomDate(random);
			contact.new_hargavebrev = GetRandomBool(random);
			contact.new_kkadminmedlemsnr = GetRandomInt(random);
			contact.new_kkadminsoegenavn = GetRandomString(random);
			contact.new_kkadminstatus = GetRandomBool(random);
			contact.new_storkredsnavn = GetRandomString(random);
			contact.new_storkredsnr = GetRandomInt(random);
			contact.new_titel = GetRandomString(random);
			contact.notat = GetRandomString(random);
			contact.telephone1 = GetRandomPhone(random);
		}

		private string GetRandomCrp(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}

		private string GetRandomLastName(Random random)
		{
			return GetRandomOfArray(random, _lastnames);
		}

		private string GetRandomFirstname(Random random)
		{
			return GetRandomOfArray(random, _firstnames);
		}
	}
}
