using System;
using System.Collections.Generic;
using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;

namespace Administration.Mapping.RandomData
{
	public class RandomAccount : AbstractRandom
	{
		private string[] _domains;
		private string[] _topDomains;
		private string[] _groupNames;
		private string[] _lastnames;
		private string[] _firstnames;
		private Dictionary<string, string> _postalCodes;

		private RandomAccount()
		{
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
		}

		private static RandomAccount Instance;
		public static RandomAccount GetInstance()
		{
			if (Instance == null)
			{
				Instance = new RandomAccount();
			}

			return Instance;
		}

		public void AnonymizeAccount(SystemInterfaceAccount account, int? seed = null)
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

			account.address1_postalcode = randomAddress.GetPostalCode(random);
			account.address1_city = randomAddress.GetCity(account.address1_postalcode);
			account.address1_line1 = GetRandomString(random);
			account.address1_line2 = GetRandomString(random);

			account.name = GetRandomString(random);

			account.emailaddress1 = randomEmail.GetRandomEmail(random, account.name);
			account.telephone1 = GetRandomPhone(random);
		}
	}
}
