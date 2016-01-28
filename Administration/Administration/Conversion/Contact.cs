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
			Convert(dynamicsCrmConnection, fromContact, toContact);

			return toContact;
		}

		public static void Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseContact fromContact, SystemInterfaceContact toContact)
		{
			toContact.createdon = fromContact.CreatedOn;
			toContact.firstname = fromContact.Firstname;
			toContact.lastname = fromContact.Lastname;
			toContact.modifiedon = fromContact.ModifiedOn;
		}
	}
}
