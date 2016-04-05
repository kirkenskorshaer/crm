using System;

namespace Utilities
{
	public class Clock
	{
		public static Func<DateTime> NowFunc = () => DateTime.Now;

		public static DateTime Now
		{
			get
			{
				return NowFunc();
			}
		}
	}
}
