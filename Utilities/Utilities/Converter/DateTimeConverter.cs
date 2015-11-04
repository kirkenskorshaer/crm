using System;
using System.Globalization;

namespace Utilities.Converter
{
	public static class DateTimeConverter
	{
		public static readonly string DateFormatShort = "yyyyMMdd";
		public static readonly string DateFormatLong = "yyyyMMdd HH:mm:ss";

		public static DateTime DateTimeFromString(string input)
		{
			DateTime convertedDateTime;
			DateTime.TryParseExact(input, DateFormatLong, null, DateTimeStyles.None, out convertedDateTime);

			return convertedDateTime;
		}
	}
}
