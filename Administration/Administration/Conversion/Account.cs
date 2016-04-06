using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using DatabaseExternalByarbejde = DataLayer.SqlData.Byarbejde.ExternalByarbejde;
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
			List<string> exclusionList = new List<string>() { "Id", "bykoordinatorid", "omraadekoordinatorid", "korshaerslederid", "genbrugskonsulentid", "byarbejdeid", "primarycontact", "kredsellerby", "region", "stedtype", "erindsamlingssted" };
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

			if (fromAccount.korshaerslederid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.korshaerslederid.Value);
				if (externalContacts.Any())
				{
					toAccount.korshaerslederid = externalContacts.First().ExternalContactId;
				}
			}

			if (fromAccount.genbrugskonsulentid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.genbrugskonsulentid.Value);
				if (externalContacts.Any())
				{
					toAccount.genbrugskonsulentid = externalContacts.First().ExternalContactId;
				}
			}

			if (fromAccount.byarbejdeid.HasValue)
			{
				List<DatabaseExternalByarbejde> externalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndByarbejde(sqlConnection, changeProviderId, fromAccount.byarbejdeid.Value);
				if (externalByarbejder.Any())
				{
					toAccount.byarbejdeid = externalByarbejder.First().ExternalByarbejdeId;
				}
			}

			if (fromAccount.primarycontact.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.primarycontact.Value);
				if (externalContacts.Any())
				{
					toAccount.primarycontact = externalContacts.First().ExternalContactId;
				}
			}

			if (fromAccount.kredsellerby.HasValue)
			{
				toAccount.kredsellerby = (SystemInterfaceAccount.kredsellerbyEnum)fromAccount.kredsellerby;
			}

			if (fromAccount.region.HasValue)
			{
				toAccount.region = (SystemInterfaceAccount.regionEnum)fromAccount.region;
			}

			if (fromAccount.erindsamlingssted.HasValue)
			{
				toAccount.erindsamlingssted = (SystemInterfaceAccount.erindsamlingsstedEnum)fromAccount.erindsamlingssted;
			}

			if (fromAccount.stedtype.HasValue)
			{
				toAccount.stedtype = (SystemInterfaceAccount.stedtypeEnum)fromAccount.stedtype;
			}
		}

		public static void Convert(SqlConnection sqlConnection, Guid changeProviderId, SystemInterfaceAccount fromAccount, DatabaseAccount toAccount)
		{
			List<string> exclusionList = new List<string>() {
				"Id",
				"new_bykoordinatorid", "bykoordinatorid",
				"new_omraadekoordinatorid", "omraadekoordinatorid",
				"new_korshaerslederid", "korshaerslederid",
				"new_genbrugskonsulentid", "genbrugskonsulentid",
				"new_byarbejdeid", "byarbejdeid",
				"primarycontactid", "primarycontact",
				"new_kredsellerby", "kredsellerby",
				"new_region", "region",
				"new_erindsamlingssted", "erindsamlingssted",
				"new_stedtype", "stedtype",
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

			if (fromAccount.korshaerslederid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.korshaerslederid.Value);
				if (externalContacts.Any())
				{
					toAccount.korshaerslederid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.genbrugskonsulentid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.genbrugskonsulentid.Value);
				if (externalContacts.Any())
				{
					toAccount.genbrugskonsulentid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.byarbejdeid.HasValue)
			{
				List<DatabaseExternalByarbejde> externalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndByarbejde(sqlConnection, changeProviderId, fromAccount.byarbejdeid.Value);
				if (externalByarbejder.Any())
				{
					toAccount.byarbejdeid = externalByarbejder.First().ByarbejdeId;
				}
			}

			if (fromAccount.primarycontact.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.primarycontact.Value);
				if (externalContacts.Any())
				{
					toAccount.primarycontact = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.kredsellerby.HasValue)
			{
				toAccount.kredsellerby = (int)fromAccount.kredsellerby;
			}

			if (fromAccount.region.HasValue)
			{
				toAccount.region = (int)fromAccount.region;
			}

			if (fromAccount.erindsamlingssted.HasValue)
			{
				toAccount.erindsamlingssted = (int)fromAccount.erindsamlingssted;
			}

			if (fromAccount.stedtype.HasValue)
			{
				toAccount.stedtype = (int)fromAccount.stedtype;
			}
		}

		public static void Convert(SqlConnection sqlConnection, Guid changeProviderId, SystemInterfaceAccount fromAccount, DatabaseAccountChange toAccount)
		{
			List<string> exclusionList = new List<string>() {
				"Id",
				"new_bykoordinatorid", "bykoordinatorid",
				"new_omraadekoordinatorid", "omraadekoordinatorid",
				"new_korshaerslederid", "korshaerslederid",
				"new_genbrugskonsulentid", "genbrugskonsulentid",
				"new_byarbejdeid", "byarbejdeid",
				"primarycontactid", "primarycontact",
				"new_kredsellerby", "kredsellerby",
				"new_region", "region",
				"new_erindsamlingssted", "erindsamlingssted",
				"new_stedtype", "stedtype",
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

			if (fromAccount.korshaerslederid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.korshaerslederid.Value);
				if (externalContacts.Any())
				{
					toAccount.korshaerslederid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.genbrugskonsulentid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.genbrugskonsulentid.Value);
				if (externalContacts.Any())
				{
					toAccount.genbrugskonsulentid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.byarbejdeid.HasValue)
			{
				List<DatabaseExternalByarbejde> externalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndByarbejde(sqlConnection, changeProviderId, fromAccount.byarbejdeid.Value);
				if (externalByarbejder.Any())
				{
					toAccount.byarbejdeid = externalByarbejder.First().ByarbejdeId;
				}
			}

			if (fromAccount.primarycontact.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.primarycontact.Value);
				if (externalContacts.Any())
				{
					toAccount.primarycontact = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.kredsellerby.HasValue)
			{
				toAccount.kredsellerby = (int)fromAccount.kredsellerby;
			}

			if (fromAccount.region.HasValue)
			{
				toAccount.region = (int)fromAccount.region;
			}

			if (fromAccount.erindsamlingssted.HasValue)
			{
				toAccount.erindsamlingssted = (int)fromAccount.erindsamlingssted;
			}

			if (fromAccount.stedtype.HasValue)
			{
				toAccount.stedtype = (int)toAccount.stedtype;
			}
		}
	}
}
