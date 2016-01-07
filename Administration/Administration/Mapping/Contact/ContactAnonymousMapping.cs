using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using Utilities.StaticData;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;

namespace Administration.Mapping.Contact
{
	public class ContactAnonymousMapping
	{
		private string[] _domains;
		private string[] _topDomains;
		private string[] _groupNames;
		private string[] _lastnames;
		private string[] _firstnames;
		private Dictionary<string, string> _postalCodes;

		private ContactAnonymousMapping()
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

			_domains = new string[]
			{
				"test",
				"experiment",
				"trial",
				"notARealEmail",
			};

			_topDomains = new string[]
			{
				"dk",
				"eu",
				"com",
				"org",
				"net",
			};

			_groupNames = new string[]
			{
				"Gave",
				"MobilePay",
				"Gaver Mariatjenesten",
				"Gaver Nicolai Tjenesten",
				"Gaver Københavnsarbejdet",
				"Gaver /Årsmærker",
				"Gaver/Korshærsbladet",
				"Juleindsamling Korshæerbladet",
				"Gave Takkebrev ønskes ikke",
				"Fonds og Legater",
				"Online-donation",
				"Gavebrev",
				"Gavebreve, procentdel",
				"Gavebreve, andre byer",
				"Høstoffer",
				"Kollekter",
				"Støtten husstandsom. Nykøbing",
				"Støtten",
				"Støtten Folkeskolebladet",
				"Støtten Husstandsomdelt",
				"Støtten Takkebrev ønskes ikke",
				"Folkeskolen/Husstand, anonym",
				"Støtten, nye ved udsend feb.98",
				"Årsmærker uden beregning",
				"Årsmærker",
				"Årsmærker med gave",
				"Jubilæumsbidrag 100 års jubilæum",
				"Jubilæumsgave KD",
				"Korshærsbl. Hovedkontor betal.",
				"Korshærsbladet Hovedkontoret",
				"Betales af Viborg",
				"Arbejdsskadeforsikring",
				"Korshærsbladet betalt 2002",
				"Korshærsbladet betalt",
				"Friab. Korshærsbladet",
				"Korshærsbladet betalt 2001",
				"Prøveabonem. Korshærsbladet",
				"Betaler for en anden",
				"Betales af butik Vejle",
				"Betales af en anden",
				"Betales af Ansgar Aalborg",
				"Korshærsbladet betalt Pensionist",
				"Julegave Korshærsblad 2004",
				"Betalt til arbejdsgrene",
				"Lodseddelsalg",
				"listeindsamling",
				"Kredsgave",
				"Basarer og fester",
				"møder og gudstjenester",
				"Butikker",
				"Diverse indbetal. og udgifter",
				"Kredssalg Årsmøde",
				"Retreat Haslev Udv. Højskole",
				"Lodsedler til storkredse",
				"Lodsedler til arbejdsgrene",
				"Lodsedler diverse",
				"Lodsedler til indenbys arb.",
				"Portoudgifter Storkredse",
				"Lodsedler Arbejdsgrenes andel",
				"Lodseddeldiff",
				"Ønsker ingen takkeskrivelse",
				"Andet (Lilian)",
				"Støttemedlemsskab",
				"Støttemedlemsskab/Husstand",
				"Støttemedlemsskab uden blade",
				"Støttemedlemsskab/Husstand uden blade",
				"Girokonto 540-1429",
				"Girokonto 308-5635",
				"Kursusbetalinger",
				"Betaling Årsmøde",
			};

			_postalCodes = new Dictionary<string, string>()
			{
				{"1120","København K"},
				{"1850","Frederiksberg C"},
				{"2500","Valby"},
				{"2600","Glostrup"},
				{"2605","Brøndby"},
				{"2610","Rødovre"},
				{"2620","Albertslund"},
				{"2625","Vallensbæk"},
				{"4200","Slagelse"},
				{"5220","Odense SØ"},
				{"5492","Vissenbjerg"},
				{"8200","Aarhus N"},
				{"9000","Aalborg"},
			};
		}

		private static ContactAnonymousMapping Instance;
		public static ContactAnonymousMapping GetInstance()
		{
			if (Instance == null)
			{
				Instance = new ContactAnonymousMapping();
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

		private string GetPostalCode(Random random)
		{
			string[] postalCodes = _postalCodes.Keys.ToArray();

			string postalCode = GetRandomOfArray(random, postalCodes);

			return postalCode;
		}

		private string GetRandomCity(SystemInterfaceContact contact)
		{
			string city = _postalCodes[contact.address1_postalcode];

			return city;
		}

		private bool GetRandomBool(Random random)
		{
			return random.Next(0, 2) == 1;
		}

		private string GetRandomCrp(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}

		private List<Group> GetRandomGroups(Random random)
		{
			int numberOfGroups = random.Next(0, 20);

			List<Group> groups = new List<Group>();

			for (int groupIndex = 0; groupIndex < numberOfGroups; groupIndex++)
			{
				Group group = new Group()
				{
					Name = GetRandomOfArray(random, _groupNames),
				};

				groups.Add(group);
			}

			//MaybeAddGroup(random, groups, "Indsamler", 30);
			//MaybeAddGroup(random, groups, "Indsamlingsleder", 5);

			groups = groups.Where(group => groups.Any(innerGroup => innerGroup.Name == group.Name && innerGroup != group) == false).ToList();

			return groups;
		}

		private static void MaybeAddGroup(Random random, List<Group> groups, string groupName, int PercentChanceToAdd)
		{
			bool isIndsamler = random.Next(0, 100) <= PercentChanceToAdd;

			if (isIndsamler)
			{
				Group group = new Group()
				{
					Name = groupName,
				};

				groups.Add(group);
			}
		}

		private string GetRandomLastName(Random random)
		{
			return GetRandomOfArray(random, _lastnames);
		}

		private string GetRandomFirstname(Random random)
		{
			return GetRandomOfArray(random, _firstnames);
		}

		private static string GetRandomOfArray(Random random, string[] options)
		{
			int optionsIndex = random.Next(0, options.Count());

			return options[optionsIndex];
		}

		private string GetRandomEmail(Random random, SystemInterfaceContact contact)
		{
			string topDomain = GetRandomOfArray(random, _topDomains);

			string domain = GetRandomOfArray(random, _domains);

			string email = $"{contact.firstname}@{domain}.{topDomain}_";

			return email;
		}

		private string GetRandomPhone(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}

		private int GetRandomInt(Random random)
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
