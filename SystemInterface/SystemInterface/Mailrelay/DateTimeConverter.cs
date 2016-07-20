using Newtonsoft.Json.Converters;
using System;
using Newtonsoft.Json;

namespace SystemInterface.Mailrelay
{
	public class DateTimeConverter : DateTimeConverterBase
	{
		private const string format = "yyyy-MM-dd hh:mm:ss";

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			DateTime parsedDateTime;

			if (reader.Value == null)
			{
				return null;
			}

			if (DateTime.TryParse(reader.Value.ToString(), out parsedDateTime))
			{
				return parsedDateTime;
			}
			return existingValue;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((DateTime)value).ToString(format));
		}
	}
}
