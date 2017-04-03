using System;
using System.Collections.Generic;
using System.Globalization;

namespace Administration.Option.Options.Logic.ImportFromKKAdminToNewModelData
{
	public abstract class AbstractKKAdminData
	{
		protected const string DateTimeCrmFormat = "yyyy-MM-dd HH:mm:ss";
		protected string DateTimeCsvFormat = "dd-MM-yyyy HH:mm:ss";

		protected void AddValueIfFilled(Dictionary<string, string> crmEntity, string key, object value)
		{
			if (value == null)
			{
				return;
			}

			if (value is string && string.IsNullOrWhiteSpace((string)value))
			{
				return;
			}

			crmEntity.Add(key, value.ToString());
		}

		protected DateTime? GetParsedDateTime(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return null;
			}

			DateTime.TryParseExact(input, DateTimeCsvFormat, null, DateTimeStyles.None, out DateTime parsedDateTime);

			return parsedDateTime;
		}
	}
}
