using System;

namespace Administration.Mapping.RandomData
{
	public class RandomEmail : AbstractRandom
	{
		private string[] _domains;
		private string[] _topDomains;

		private RandomEmail()
		{
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
		}

		internal string GetRandomEmail(Random random, string name)
		{
			string topDomain = GetRandomOfArray(random, _topDomains);

			string domain = GetRandomOfArray(random, _domains);

			string email = $"{name}@{domain}.{topDomain}_";

			return email;
		}

		private static RandomEmail Instance;
		public static RandomEmail GetInstance()
		{
			if (Instance == null)
			{
				Instance = new RandomEmail();
			}

			return Instance;
		}
	}
}
