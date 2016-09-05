using System;

namespace Utilities
{
	public static class DateTimeHelper
	{
		public static bool Between(DateTime evaluateDateTime, DateTime from, DateTime to)
		{
			if (from > to)
			{
				return Between(evaluateDateTime, to, from);
			}

			return from <= evaluateDateTime && evaluateDateTime <= to;
		}
	}
}
