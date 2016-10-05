using System;
using System.Collections.Generic;
using System.Text;
using Utilities.StaticData;

namespace TestUtilities
{
	public static class GetTestData
	{
		private static Random _random = new Random();
		private static string _characters = "qwertyuiopåasdfghjklæøzxcvbnm";

		public static KeyValuePair<string, string> GetPostalCity()
		{
			List<KeyValuePair<string, string>> postalCodes = PostalCity.GetInstance().CityPostalCode;

			return postalCodes[_random.Next(0, postalCodes.Count - 1)];
		}

		public static string GetName()
		{
			return GetName(5, 10);
		}

		public static string GetName(int lengthMin, int lengthMax)
		{
			int length = _random.Next(lengthMin, lengthMax);

			return GetName(length);
		}

		public static string GetName(int length)
		{
			StringBuilder nameBuilder = new StringBuilder();

			for (int sizeIndex = 0; sizeIndex < length; sizeIndex++)
			{
				if (sizeIndex == 0)
				{
					nameBuilder.Append(_characters[_random.Next(0, _characters.Length - 1)].ToString().ToUpper());
					continue;
				}

				nameBuilder.Append(_characters[_random.Next(0, _characters.Length - 1)]);
			}

			return nameBuilder.ToString();
		}

		public static string GetEmail(int lengthMin, int lengthMax)
		{
			return GetName(lengthMin, lengthMax) + "@korsnet.dk";
		}

		public static string GetEmail(int length)
		{
			return GetName(length) + "@korsnet.dk";
		}

		public static string GetEmail()
		{
			return GetName().Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa") + "@korsnet.dk";
		}

		public static string GetPhone()
		{
			return _random.Next(10000000, 99999999).ToString();
		}
	}
}
