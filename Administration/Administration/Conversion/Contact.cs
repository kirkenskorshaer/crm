using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using SystemInterface.Dynamics.Crm;

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
	}
}
