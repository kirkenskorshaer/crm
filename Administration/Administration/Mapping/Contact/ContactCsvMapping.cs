using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DataBaseContact = DataLayer.SqlData.Contact.Contact;
using DataBaseContactChange = DataLayer.SqlData.Contact.ContactChange;

namespace Administration.Mapping.Contact
{
	public static class ContactCsvMapping
	{
		public static DataBaseContact FindContact(SqlConnection sqlConnection, Guid externalContactId, string firstName)
		{
			List<DataBaseContact> contacts = DataBaseContact.Read(sqlConnection, firstName);
			if(contacts.Count == 1)
			{
				return contacts[0];
			}

			List<DataBaseContact> contactsPreviouslyChanged = DataBaseContactChange.GetContacts(sqlConnection, externalContactId);

			if(contacts.Count == 0)
			{
				if (contactsPreviouslyChanged.Count == 0)
				{
                    return null;
				}

				if (contactsPreviouslyChanged.Count == 1)
				{
					return contactsPreviouslyChanged[0];
				}
			}

			foreach (DataBaseContact contact in contacts)
			{
				if (contactsPreviouslyChanged.Contains(contact))
				{
					return contact;
				}
			}

			return contacts.OrderByDescending(contact => contact.ModifiedOn).First();
        }
	}
}
