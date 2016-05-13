using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using SystemInterface.Dynamics.Crm;
using DataLayer.MongoData.Input;
using DataLayer.SqlData.Contact;

namespace Administration.Conversion
{
	public static class Contact
	{
		public static SystemInterfaceContact Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseContact fromContact)
		{
			SystemInterfaceContact toContact = new SystemInterfaceContact(dynamicsCrmConnection);
			Convert(fromContact, toContact);

			return toContact;
		}

		public static void Convert(DatabaseContact fromContact, SystemInterfaceContact toContact)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseContact), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromContact, key);
				Utilities.ReflectionHelper.SetValue(toContact, key, value);
			}
		}

		public static void Convert(SystemInterfaceContact fromContact, DatabaseContact toContact)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceContact), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromContact, key);
				Utilities.ReflectionHelper.SetValue(toContact, key, value);
			}
		}

		public static void Convert(SystemInterfaceContact fromContact, DatabaseContactChange toContact)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceContact), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromContact, key);
				Utilities.ReflectionHelper.SetValue(toContact, key, value);
			}
		}

		public static void Convert(Stub fromStub, DatabaseContactChange toContactChange)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keysInContactChange = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseContactChange), exclusionList);
			List<string> keysInStub = fromStub.Contents.Select(stub => stub.Key).ToList();

			List<string> keys = keysInStub.Intersect(keysInContactChange).ToList();

			foreach (string key in keys)
			{
				string valueString = fromStub.Contents.Where(content => content.Key == key).FirstOrDefault().Value;

				Type objectType = Utilities.ReflectionHelper.GetType(toContactChange, key);
				object newValue = null;

				switch (objectType.Name)
				{
					case "String":
						newValue = valueString;
						break;
					case "Int32":
						int valueInt = 0;
						int.TryParse(valueString, out valueInt);
						newValue = valueInt;
						break;
					case "Boolean":
						bool valueBool = false;
						bool.TryParse(valueString, out valueBool);
						newValue = valueBool;
						break;
					default:
						break;
				}

				Utilities.ReflectionHelper.SetValue(toContactChange, key, newValue);
			}
		}
	}
}
