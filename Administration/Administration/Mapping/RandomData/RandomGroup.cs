using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace Administration.Mapping.RandomData
{
	public class RandomGroup : AbstractRandom
	{
		private string[] _groupNames;
		
		private RandomGroup()
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

		private static RandomGroup Instance;
		public static RandomGroup GetInstance()
		{
			if (Instance == null)
			{
				Instance = new RandomGroup();
			}

			return Instance;
		}

		internal List<Group> GetRandomGroups()
		{
			Random random = new Random();
			return GetRandomGroups(random);
		}

		internal List<Group> GetRandomGroups(Random random)
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

			groups = groups.Where(group => groups.Any(innerGroup => innerGroup.Name == group.Name && innerGroup != group) == false).ToList();

			return groups;
		}
	}
}
