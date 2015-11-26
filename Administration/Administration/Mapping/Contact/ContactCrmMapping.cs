using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DataLayer;
using DataBaseContact = DataLayer.SqlData.Contact.Contact;
using DataBaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DataBaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DataBaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;

namespace Administration.Mapping.Contact
{
	public static class ContactCrmMapping
	{
		public static DataBaseContact FindContact(MongoConnection mongoConnection, SqlConnection sqlConnection, Guid externalContactId, SystemInterfaceContact crmContact)
		{
			List<DataBaseContact> contactsPreviouslyChanged = DataBaseContactChange.GetContacts(sqlConnection, externalContactId);

			if (contactsPreviouslyChanged.Count == 0)
			{
				return null;
			}

			if (contactsPreviouslyChanged.Count == 1)
			{
				return contactsPreviouslyChanged[0];
			}

			Log.Write(mongoConnection, $"multible contact match for {externalContactId}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			return contactsPreviouslyChanged.OrderByDescending(contact => contact.CreatedOn).First();
		}

		public static List<DataBaseExternalContact> FindContacts(MongoConnection connection, SqlConnection sqlConnection, DataBaseContact contact, Guid changeProviderId)
		{
			Guid contactId = contact.Id;

			List<DataBaseExternalContact> externalContacts = DataBaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, contactId);

			if (externalContacts.Count == 0)
			{
				Log.Write(connection, $"no external contact found for changeProvider {changeProviderId} and contact {contactId}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}

			if (externalContacts.Count > 1)
			{
				Log.Write(connection, $"more than one external contacts found on changeProvider {0} and contact {contactId}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}

			return externalContacts;
		}
	}
}
