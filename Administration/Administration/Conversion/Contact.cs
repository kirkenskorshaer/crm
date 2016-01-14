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
			return Convert(dynamicsCrmConnection, fromContact, toContact);
		}

		public static SystemInterfaceContact Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseContact fromContact, SystemInterfaceContact toContact)
		{
			SystemInterfaceContact systemInterfaceContact = new SystemInterfaceContact(dynamicsCrmConnection)
			{
				createdon = fromContact.CreatedOn,
				firstname = fromContact.Firstname,
				lastname = fromContact.Lastname,
				modifiedon = fromContact.ModifiedOn,
			};

			return systemInterfaceContact;
		}
	}
}
