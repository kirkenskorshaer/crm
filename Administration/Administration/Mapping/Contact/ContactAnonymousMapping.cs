using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using Utilities.StaticData;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;

namespace Administration.Mapping.Contact
{
	public static class ContactAnonymousMapping
	{
		public static void AnonymizeContact(SystemInterfaceContact contact, int? seed = null)
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

			contact.address1_postalcode = GetPostalCode(random);
			contact.address1_city = GetRandomCity(contact);
			contact.address1_line1 = GetRandomString(random);
			contact.address1_line2 = GetRandomString(random);
			contact.birthdate = GetRandomDate(random);
			contact.firstname = GetRandomFirstname(random);
			contact.lastname = GetRandomLastName(random);
			contact.emailaddress1 = GetRandomEmail(random, contact);
			contact.Groups = GetRandomGroups(random);
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

		private static string GetPostalCode(Random random)
		{
			string[] postalCodes = PostalCity.CityPostalCode.Keys.ToArray();

			string postalCode = GetRandomOfArray(random, postalCodes);

			return postalCode;
		}

		private static string GetRandomCity(SystemInterfaceContact contact)
		{
			string city = PostalCity.CityPostalCode[contact.address1_postalcode];

			return city;
		}

		private static bool GetRandomBool(Random random)
		{
			return random.Next(0, 2) == 1;
		}

		private static string GetRandomCrp(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}

		private static List<Group> GetRandomGroups(Random random)
		{
			int groupNumer = random.Next(0, 100);

			List<Group> groups = new List<Group>();

			for (int groupIndex = 0; groupIndex <= groupNumer; groupIndex++)
			{
				Group group = new Group();

				switch(groupIndex)
				{
					case 30:
						group.Name = "Indsamler";
						break;
					case 80:
						group.Name = "Indsamlingsleder";
						break;
					default:
                        group.Name = $"group {groupIndex}";
						break;
				};

				groups.Add(group);
			}

			return groups;
		}

		private static string GetRandomLastName(Random random)
		{
			string[] lastnames = new string[]
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

			return GetRandomOfArray(random, lastnames);
		}

		private static string GetRandomFirstname(Random random)
		{
			string[] firstnames = new string[]
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

			return GetRandomOfArray(random, firstnames);
		}

		private static string GetRandomOfArray(Random random, string[] options)
		{
			int optionsIndex = random.Next(0, options.Count());

			return options[optionsIndex];
		}

		private static string GetRandomEmail(Random random, SystemInterfaceContact contact)
		{
			string[] topDomains = new string[]
			{
				"dk",
				"eu",
				"com",
				"org",
				"net",
			};

			string topDomain = GetRandomOfArray(random, topDomains);

			string[] domains = new string[]
			{
				"test",
				"experiment",
				"trial",
				"notARealEmail",
			};

			string domain = GetRandomOfArray(random, topDomains);

			string email = $"{contact.firstname}@{domain}.{topDomain}_";

			return email;
		}

		private static string GetRandomPhone(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}

		private static int GetRandomInt(Random random)
		{
			return random.Next();
		}

		private static DateTime? GetRandomDate(Random random)
		{
			int year = random.Next(1971, 2200);
			int month = random.Next(1, 13);
			int day = random.Next(1, 27);

			return new DateTime(year, month, day);
		}

		private static string GetRandomString(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}
	}
}
