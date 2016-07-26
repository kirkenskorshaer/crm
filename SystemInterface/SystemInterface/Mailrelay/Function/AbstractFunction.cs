using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using SystemInterface.Mailrelay.Function;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay
{
	public abstract class AbstractFunction
	{
		public string apiKey;

		[ToGet(type = ToGetAttribute.typeEnum.ignore)]
		public abstract Type ReturnType { get; }

		public string ToGet()
		{
			StringBuilder objectAsGetStringBuilder = new StringBuilder();

			MemberInfo[] allMembers = GetType().GetMembers();
			List<MemberInfo> members = allMembers.Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property).ToList();

			string function = GetType().Name;

			AddUrlEncoded(objectAsGetStringBuilder, "function", function);

			foreach (MemberInfo member in members)
			{
				ToGetAttribute toGetAttribute = (ToGetAttribute)member.GetCustomAttributes(typeof(ToGetAttribute)).SingleOrDefault();

				if (toGetAttribute != null && toGetAttribute.type == ToGetAttribute.typeEnum.ignore)
				{
					continue;
				}

				string getName = FindGetName(member, toGetAttribute);

				object value = Utilities.ReflectionHelper.GetValue(this, member);

				if (value == null)
				{
					continue;
				}

				if (AddCollectionType(getName, value, objectAsGetStringBuilder))
				{
					continue;
				}

				string valueString = FindGetValue(value, toGetAttribute);

				if (string.IsNullOrWhiteSpace(valueString))
				{
					continue;
				}

				AddUrlEncoded(objectAsGetStringBuilder, getName, valueString);
			}

			return objectAsGetStringBuilder.ToString();
		}

		private bool AddCollectionType(string getName, object value, StringBuilder objectAsGetStringBuilder)
		{
			if (value is IEnumerable<int>)
			{
				AddIEnumerableInt(value, getName, objectAsGetStringBuilder);
				return true;
			}

			if (value is IEnumerable<string>)
			{
				AddIEnumerableString(value, getName, objectAsGetStringBuilder);
				return true;
			}

			if (value.GetType() == typeof(Dictionary<string, string>))
			{
				AddDictionary(value, getName, objectAsGetStringBuilder);
				return true;
			}

			return false;
		}

		private bool AddCollectionType(string getName, object value, Dictionary<string, string> values)
		{
			if (value is IEnumerable<int>)
			{
				AddIEnumerableInt(value, getName, values);
				return true;
			}

			if (value is IEnumerable<string>)
			{
				AddIEnumerableString(value, getName, values);
				return true;
			}

			if (value.GetType() == typeof(Dictionary<string, string>))
			{
				AddDictionary(value, getName, values);
				return true;
			}

			return false;
		}

		private void AddDictionary(object value, string getName, Dictionary<string, string> values)
		{
			Dictionary<string, string> valueDictionary = value as Dictionary<string, string>;

			foreach (KeyValuePair<string, string> keyValuePair in valueDictionary)
			{
				string currentName = $"{getName}[{keyValuePair.Key}]";
				values.Add(currentName, $"{keyValuePair.Value}");
			}
		}

		private void AddDictionary(object value, string getName, StringBuilder objectAsGetStringBuilder)
		{
			Dictionary<string, string> valueDictionary = value as Dictionary<string, string>;

			foreach (KeyValuePair<string, string> keyValuePair in valueDictionary)
			{
				string currentName = $"{getName}[{keyValuePair.Key}]";
				AddUrlEncoded(objectAsGetStringBuilder, currentName, $"{keyValuePair.Value}");
			}
		}

		private void AddIEnumerableInt(object value, string getName, StringBuilder objectAsGetStringBuilder)
		{
			IEnumerable<int> valueIEnumerable = value as IEnumerable<int>;

			if (valueIEnumerable == null)
			{
				return;
			}

			int index = 0;
			foreach (int currentInt in valueIEnumerable)
			{
				string currentName = $"{getName}[{index}]";
				AddUrlEncoded(objectAsGetStringBuilder, currentName, currentInt.ToString());
				index++;
			}
		}

		private void AddIEnumerableInt(object value, string getName, Dictionary<string, string> values)
		{
			IEnumerable<int> valueIEnumerable = value as IEnumerable<int>;

			if (valueIEnumerable == null)
			{
				return;
			}

			int index = 0;
			foreach (int currentInt in valueIEnumerable)
			{
				string currentName = $"{getName}[{index}]";
				values.Add(currentName, currentInt.ToString());
				index++;
			}
		}

		private void AddIEnumerableString(object value, string getName, Dictionary<string, string> values)
		{
			IEnumerable<string> valueIEnumerable = value as IEnumerable<string>;

			if (valueIEnumerable == null)
			{
				return;
			}

			int index = 0;
			foreach (string currentString in valueIEnumerable)
			{
				string currentName = $"{getName}[{index}]";
				values.Add(currentName, $"{currentString}");
				index++;
			}
		}

		private void AddIEnumerableString(object value, string getName, StringBuilder objectAsGetStringBuilder)
		{
			IEnumerable<string> valueIEnumerable = value as IEnumerable<string>;

			if (valueIEnumerable == null)
			{
				return;
			}

			int index = 0;
			foreach (string currentString in valueIEnumerable)
			{
				string currentName = $"{getName}[{index}]";
				AddUrlEncoded(objectAsGetStringBuilder, currentName, $"{currentString}");
				index++;
			}
		}

		private string FindGetName(MemberInfo member, ToGetAttribute toGetAttribute)
		{
			if (toGetAttribute == null)
			{
				return member.Name;
			}

			if (string.IsNullOrWhiteSpace(toGetAttribute.name))
			{
				return member.Name;
			}

			return toGetAttribute.name;
		}

		private string FindGetValue(object value, ToGetAttribute toGetAttribute)
		{
			if (value == null)
			{
				return null;
			}

			if (toGetAttribute == null)
			{
				return ValueToString(value);
			}

			if (toGetAttribute.type == ToGetAttribute.typeEnum.intEnum)
			{
				return ((int)value).ToString();
			}

			if (toGetAttribute.type == ToGetAttribute.typeEnum.ShortDate)
			{
				return ((DateTime)value).ToString("yyyy-MM-dd");
			}

			return ValueToString(value);
		}

		private string ValueToString(object value)
		{
			string valueString;

			Type valueType = value.GetType();

			if (valueType == typeof(DateTime))
			{
				valueString = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
			}
			else if (valueType == typeof(bool) || valueType == typeof(bool?))
			{
				if ((bool?)value == true)
				{
					valueString = "1";
				}
				else
				{
					valueString = "0";
				}
			}
			else
			{
				valueString = value.ToString();
			}

			return valueString;
		}

		private void AddUrlEncoded(StringBuilder objectAsGetStringBuilder, string name, string valueString)
		{
			objectAsGetStringBuilder.Append("&");
			objectAsGetStringBuilder.Append(HttpUtility.UrlEncode(name));
			objectAsGetStringBuilder.Append("=");
			objectAsGetStringBuilder.Append(HttpUtility.UrlEncode(valueString));
		}

		public AbstractMailrelayReply GetMailrelayReply(string replyString)
		{
			AbstractMailrelayReply reply = JsonConvert.DeserializeObject(replyString, ReturnType, new DateTimeConverter()) as AbstractMailrelayReply;

			return reply;
		}

		public enum sortOrderEnum
		{
			ASC = 1,
			DESC = 2,
		}

		public Dictionary<string, string> GetValues()
		{
			Dictionary<string, string> values = new Dictionary<string, string>();

			MemberInfo[] allMembers = GetType().GetMembers();
			List<MemberInfo> members = allMembers.Where(member => member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property).ToList();

			string function = GetType().Name;

			values.Add("function", function);

			foreach (MemberInfo member in members)
			{
				ToGetAttribute toGetAttribute = (ToGetAttribute)member.GetCustomAttributes(typeof(ToGetAttribute)).SingleOrDefault();

				if (toGetAttribute != null && toGetAttribute.type == ToGetAttribute.typeEnum.ignore)
				{
					continue;
				}

				string getName = FindGetName(member, toGetAttribute);

				object value = Utilities.ReflectionHelper.GetValue(this, member);

				if (value == null)
				{
					continue;
				}

				if (AddCollectionType(getName, value, values))
				{
					continue;
				}

				string valueString = FindGetValue(value, toGetAttribute);

				if (string.IsNullOrWhiteSpace(valueString))
				{
					continue;
				}

				values.Add(getName, valueString);
			}

			return values;
		}
	}
}
