﻿using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
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

		public static void Convert(SqlConnection sqlConnection, Guid changeProviderId, SystemInterfaceAccount fromAccount, DatabaseAccount toAccount)
		{
			List<string> exclusionList = new List<string>() {
				"Id",
				"new_bykoordinatorid", "bykoordinatorid",
				"new_omraadekoordinatorid", "omraadekoordinatorid",
				"new_kredsellerby", "kredsellerby"
			};

			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceAccount), exclusionList);

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
					toAccount.bykoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.omraadekoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.omraadekoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.omraadekoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.kredsellerby.HasValue)
			{
				toAccount.kredsellerby = (int)fromAccount.kredsellerby;
			}
		}

		public static void Convert(SqlConnection sqlConnection, Guid changeProviderId, SystemInterfaceAccount fromAccount, DatabaseAccountChange toAccount)
		{
			List<string> exclusionList = new List<string>() {
				"Id",
				"new_bykoordinatorid", "bykoordinatorid",
				"new_omraadekoordinatorid", "omraadekoordinatorid",
				"new_kredsellerby", "kredsellerby"
			};

			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceAccount), exclusionList);

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
					toAccount.bykoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.omraadekoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.omraadekoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.omraadekoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.kredsellerby.HasValue)
			{
				toAccount.kredsellerby = (int)fromAccount.kredsellerby;
			}
		}
	}
}
