using System;
using System.Collections.Generic;
using System.Linq;

namespace Administration.Mapping.RandomData
{
	public class RandomAddress : AbstractRandom
	{
		private Dictionary<string, string> _postalCodes;

		private RandomAddress()
		{
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

		private static RandomAddress Instance;
		public static RandomAddress GetInstance()
		{
			if (Instance == null)
			{
				Instance = new RandomAddress();
			}

			return Instance;
		}

		internal string GetPostalCode(Random random)
		{
			string[] postalCodes = _postalCodes.Keys.ToArray();

			string postalCode = GetRandomOfArray(random, postalCodes);

			return postalCode;
		}

		internal string GetCity(string postalCode)
		{
			string city = _postalCodes[postalCode];

			return city;
		}
	}
}
