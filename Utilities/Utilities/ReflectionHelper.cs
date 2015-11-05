using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities
{
	public class ReflectionHelper
	{
		public static List<string> GetFieldsAndProperties(Type holderType, List<string> exclusionList)
		{
			List<string> fieldsAndProperties = new List<string>();

			FieldInfo[] fieldsInfo = holderType.GetFields(BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo[] propertiesInfo = holderType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			fieldsAndProperties.AddRange(fieldsInfo.Select(info => info.Name));
			fieldsAndProperties.AddRange(propertiesInfo.Select(info => info.Name));

			fieldsAndProperties = fieldsAndProperties.Except(exclusionList).ToList();

			return fieldsAndProperties;
		}

		public static object GetValue(object holderObject, string fieldOrPropertyName)
		{
			Type holderType = holderObject.GetType();

			FieldInfo fieldInfo = holderType.GetField(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo != null)
			{
				return fieldInfo.GetValue(holderObject);
			}

			PropertyInfo propertyInfo = holderType.GetProperty(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			return propertyInfo.GetValue(holderObject);
		}

		public static void SetValue(object holderObject, string fieldOrPropertyName, object value)
		{
			Type holderType = holderObject.GetType();

			FieldInfo fieldInfo = holderType.GetField(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo != null)
			{
				fieldInfo.SetValue(holderObject, value);
				return;
			}

			PropertyInfo propertyInfo = holderType.GetProperty(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			propertyInfo.SetValue(holderObject, value);
		}
	}
}
