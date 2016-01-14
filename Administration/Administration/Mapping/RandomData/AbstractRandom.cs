using System;
using System.Linq;

namespace Administration.Mapping.RandomData
{
	public abstract class AbstractRandom
	{
		protected bool GetRandomBool(Random random)
		{
			return random.Next(0, 2) == 1;
		}

		public bool GetRandomBool(Random random, int PercentChanceForTrue)
		{
			bool isTrue = random.Next(0, 100) <= PercentChanceForTrue;

			return isTrue;
		}

		protected string GetRandomOfArray(Random random, string[] options)
		{
			int optionsIndex = random.Next(0, options.Count());

			return options[optionsIndex];
		}

		protected int GetRandomInt(Random random)
		{
			return random.Next();
		}

		protected DateTime? GetRandomDate(Random random)
		{
			int year = random.Next(1971, 2200);
			int month = random.Next(1, 13);
			int day = random.Next(1, 27);

			return new DateTime(year, month, day);
		}

		protected string GetRandomPhone(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}

		protected string GetRandomString(Random random)
		{
			return random.Next(0, 1000000).ToString("000000");
		}
	}
}
