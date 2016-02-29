using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using SystemInterface.Dynamics.Crm;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Linq;

namespace Administration.Conversion
{
	public static class Account
	{
		public static SystemInterfaceAccount Convert(DynamicsCrmConnection dynamicsCrmConnection, SqlConnection sqlConnection, Guid changeProviderId, DatabaseAccount fromAccount)
		{
			SystemInterfaceAccount toAccount = new SystemInterfaceAccount(dynamicsCrmConnection);
			Convert(sqlConnection, changeProviderId, fromAccount, toAccount);

			return toAccount;
		}

		public static void Convert(SqlConnection sqlConnection, Guid changeProviderId, DatabaseAccount fromAccount, SystemInterfaceAccount toAccount)
		{
			List<string> exclusionList = new List<string>() { "Id", "bykoordinatorid", "omraadekoordinatorid", "kredsellerby" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseAccount), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAccount, key);
				Utilities.ReflectionHelper.SetValue(toAccount, key, value);
			}

			if (fromAccount.bykoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.bykoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.bykoordinatorid = externalContacts.First().ExternalContactId;
				}
			}

			if (fromAccount.omraadekoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.omraadekoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.omraadekoordinatorid = externalContacts.First().ExternalContactId;
				}
			}

			if (fromAccount.kredsellerby.HasValue)
			{
				toAccount.kredsellerby = (SystemInterfaceAccount.kredsellerbyEnum)fromAccount.kredsellerby;
			}
		}
	}
}
