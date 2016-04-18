using DataLayer.SqlData.Annotation;
using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;
using SystemInterfaceAnnotation = SystemInterface.Dynamics.Crm.Annotation;
using System;

namespace Administration.Conversion
{
	public static class Annotation
	{
		public static SystemInterfaceAnnotation Convert(DynamicsCrmConnection dynamicsCrmConnection, ContactAnnotation fromAnnotation)
		{
			SystemInterfaceAnnotation toAnnotation = new SystemInterfaceAnnotation(dynamicsCrmConnection);
			Convert(fromAnnotation, toAnnotation);

			return toAnnotation;
		}

		public static void Convert(ContactAnnotation fromAnnotation, SystemInterfaceAnnotation toAnnotation)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(ContactAnnotation), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAnnotation, key);
				Utilities.ReflectionHelper.SetValue(toAnnotation, key, value);
			}
		}

		public static void Convert(SystemInterfaceAnnotation fromAnnotation, ContactAnnotation toAnnotation)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceAnnotation), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAnnotation, key);
				Utilities.ReflectionHelper.SetValue(toAnnotation, key, value);
			}
		}

		public static SystemInterfaceAnnotation Convert(DynamicsCrmConnection dynamicsCrmConnection, AccountAnnotation fromAnnotation)
		{
			SystemInterfaceAnnotation toAnnotation = new SystemInterfaceAnnotation(dynamicsCrmConnection);
			Convert(fromAnnotation, toAnnotation);

			return toAnnotation;
		}

		public static void Convert(AccountAnnotation fromAnnotation, SystemInterfaceAnnotation toAnnotation)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(AccountAnnotation), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAnnotation, key);
				Utilities.ReflectionHelper.SetValue(toAnnotation, key, value);
			}
		}

		public static void Convert(SystemInterfaceAnnotation fromAnnotation, AccountAnnotation toAnnotation)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceAnnotation), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAnnotation, key);
				Utilities.ReflectionHelper.SetValue(toAnnotation, key, value);
			}
		}

		internal static void Convert(SystemInterfaceAnnotation fromAnnotation, ContactChangeAnnotation toAnnotation)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceAnnotation), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAnnotation, key);
				Utilities.ReflectionHelper.SetValue(toAnnotation, key, value);
			}
		}

		internal static void Convert(SystemInterfaceAnnotation fromAnnotation, AccountChangeAnnotation toAnnotation)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceAnnotation), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAnnotation, key);
				Utilities.ReflectionHelper.SetValue(toAnnotation, key, value);
			}
		}
	}
}
