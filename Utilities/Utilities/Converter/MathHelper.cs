using System.Collections.Generic;
using System.Linq;

namespace Utilities.Converter
{
	public static class MathHelper
	{
		public static int DigitSum(int number)
		{
			IEnumerable<int> numbers = number.ToString().Select(digit => int.Parse(digit.ToString()));

			int sum = 0;
			foreach(int digit in numbers)
			{
				sum += digit;
			}

			if(sum < 10)
			{
				return sum;
			}

			return DigitSum(sum);
		}
	}
}
