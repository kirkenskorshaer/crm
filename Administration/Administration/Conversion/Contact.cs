using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;

namespace Administration.Conversion
{
	public static class Contact
	{
		public static SystemInterfaceContact Convert(DatabaseContact fromContact, Guid ExternalContactIdFrom)
		{
			SystemInterfaceContact toContact = new SystemInterfaceContact();
			return Convert(fromContact, ExternalContactIdFrom, toContact);
		}

		public static SystemInterfaceContact Convert(DatabaseContact fromContact)
		{
			SystemInterfaceContact toContact = new SystemInterfaceContact();
			return Convert(fromContact, toContact);
		}

		public static SystemInterfaceContact Convert(DatabaseContact fromContact, Guid ExternalContactIdFrom, SystemInterfaceContact toContact)
		{
			SystemInterfaceContact systemInterfaceContact = Convert(fromContact, toContact);
			systemInterfaceContact.contactid = ExternalContactIdFrom;
			return systemInterfaceContact;
		}

		public static SystemInterfaceContact Convert(DatabaseContact fromContact, SystemInterfaceContact toContact)
		{
			SystemInterfaceContact systemInterfaceContact = new SystemInterfaceContact()
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
