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

		public static Type GetType(object holderObject, string fieldOrPropertyName)
		{
			Type holderType = holderObject.GetType();

			FieldInfo fieldInfo = holderType.GetField(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo != null)
			{
				return fieldInfo.FieldType;
			}

			PropertyInfo propertyInfo = holderType.GetProperty(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			if (propertyInfo == null)
			{
				throw new ArgumentException($"failed to get type from {holderObject}.{fieldOrPropertyName}");
			}

			return propertyInfo.PropertyType;
		}

		public static bool SetValue(object holderObject, string fieldOrPropertyName, object value)
		{
			Type holderType = holderObject.GetType();

			FieldInfo fieldInfo = holderType.GetField(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			if (fieldInfo != null)
			{
				fieldInfo.SetValue(holderObject, value);
				return true;
			}

			PropertyInfo propertyInfo = holderType.GetProperty(fieldOrPropertyName, BindingFlags.Public | BindingFlags.Instance);

			if (propertyInfo == null)
			{
				return false;
			}

			propertyInfo.SetValue(holderObject, value);

			return true;
		}

		public static object StringToObject(string from, Type targetType)
		{
			string fullName = targetType.FullName;

			if (fullName == typeof(String).FullName)
			{
				return from;
			}

			if (fullName == typeof(Int32).FullName)
			{
				int valueInt = 0;
				int.TryParse(from, out valueInt);
				return valueInt;
			}

			if (fullName == typeof(Boolean).FullName)
			{
				bool valueBool = false;
				bool.TryParse(from, out valueBool);
				return valueBool;
			}

			if (fullName == typeof(Guid?).FullName)
			{
				Guid valueGuid = Guid.Empty;
				Guid.TryParse(from, out valueGuid);
				return (Guid?)valueGuid;
			}

			throw new Exception($"unknown type {fullName}");
		}

		public static void Copy(object from, object to, List<string> exclusionList)
		{
			List<string> keys = GetFieldsAndProperties(from.GetType(), exclusionList);

			foreach (string key in keys)
			{
				object value = GetValue(from, key);
				SetValue(to, key, value);
			}
		}
	}
}
