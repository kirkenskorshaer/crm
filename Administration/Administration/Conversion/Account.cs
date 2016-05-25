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
using DataLayer.MongoData.Input;

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
			List<string> exclusionList = new List<string>() { "Id", "bykoordinatorid", "omraadekoordinatorid", "korshaerslederid", "genbrugskonsulentid", "indsamlingskoordinatorid", "byarbejdeid", "primarycontact", "kredsellerby", "region", "stedtype", "erindsamlingssted" };
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

			if (fromAccount.indsamlingskoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndContact(sqlConnection, changeProviderId, fromAccount.indsamlingskoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.indsamlingskoordinatorid = externalContacts.First().ExternalContactId;
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
				"new_indsamlingskoordinatorid","indsamlingskoordinatorid",
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
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.bykoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.bykoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.omraadekoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.omraadekoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.omraadekoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.korshaerslederid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.korshaerslederid.Value);
				if (externalContacts.Any())
				{
					toAccount.korshaerslederid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.genbrugskonsulentid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.genbrugskonsulentid.Value);
				if (externalContacts.Any())
				{
					toAccount.genbrugskonsulentid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.indsamlingskoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.indsamlingskoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.indsamlingskoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.byarbejdeid.HasValue)
			{
				List<DatabaseExternalByarbejde> externalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndExternalByarbejde(sqlConnection, changeProviderId, fromAccount.byarbejdeid.Value);
				if (externalByarbejder.Any())
				{
					toAccount.byarbejdeid = externalByarbejder.First().ByarbejdeId;
				}
			}

			if (fromAccount.primarycontact.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.primarycontact.Value);
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
				"new_indsamlingskoordinatorid", "indsamlingskoordinatorid",
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
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.bykoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.bykoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.omraadekoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.omraadekoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.omraadekoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.korshaerslederid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.korshaerslederid.Value);
				if (externalContacts.Any())
				{
					toAccount.korshaerslederid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.genbrugskonsulentid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.genbrugskonsulentid.Value);
				if (externalContacts.Any())
				{
					toAccount.genbrugskonsulentid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.indsamlingskoordinatorid.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.indsamlingskoordinatorid.Value);
				if (externalContacts.Any())
				{
					toAccount.indsamlingskoordinatorid = externalContacts.First().ContactId;
				}
			}

			if (fromAccount.byarbejdeid.HasValue)
			{
				List<DatabaseExternalByarbejde> externalByarbejder = DatabaseExternalByarbejde.ReadFromChangeProviderAndExternalByarbejde(sqlConnection, changeProviderId, fromAccount.byarbejdeid.Value);
				if (externalByarbejder.Any())
				{
					toAccount.byarbejdeid = externalByarbejder.First().ByarbejdeId;
				}
			}

			if (fromAccount.primarycontact.HasValue)
			{
				List<DatabaseExternalContact> externalContacts = DatabaseExternalContact.ReadFromChangeProviderAndExternalContact(sqlConnection, changeProviderId, fromAccount.primarycontact.Value);
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

		public static void Convert(Stub fromStub, DatabaseAccountChange toAccountChange)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keysInAccountChange = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseAccountChange), exclusionList);
			List<string> keysInStub = fromStub.Contents.Select(stub => stub.Key).ToList();

			List<string> keys = keysInStub.Intersect(keysInAccountChange).ToList();

			foreach (string key in keys)
			{
				string valueString = fromStub.Contents.Where(content => content.Key == key).FirstOrDefault().Value;

				Type objectType = Utilities.ReflectionHelper.GetType(toAccountChange, key);
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

				Utilities.ReflectionHelper.SetValue(toAccountChange, key, newValue);
			}
		}

		public static IndsamlerDefinition.IndsamlerTypeEnum Convert(DataLayer.SqlData.Account.AccountIndsamler.IndsamlerTypeEnum indsamlerType)
		{
			switch (indsamlerType)
			{
				case DataLayer.SqlData.Account.AccountIndsamler.IndsamlerTypeEnum.Indsamlingshjaelper:
					return IndsamlerDefinition.IndsamlerTypeEnum.Indsamlingshjaelper;
				case DataLayer.SqlData.Account.AccountIndsamler.IndsamlerTypeEnum.Indsamler:
					return IndsamlerDefinition.IndsamlerTypeEnum.Indsamler;
				default:
					throw new Exception($"Cannot convert {indsamlerType}");
			}
		}

		public static DataLayer.SqlData.Account.AccountIndsamler.IndsamlerTypeEnum Convert(IndsamlerDefinition.IndsamlerTypeEnum indsamlerType)
		{
			switch (indsamlerType)
			{
				case IndsamlerDefinition.IndsamlerTypeEnum.Indsamlingshjaelper:
					return DataLayer.SqlData.Account.AccountIndsamler.IndsamlerTypeEnum.Indsamlingshjaelper;
				case IndsamlerDefinition.IndsamlerTypeEnum.Indsamler:
					return DataLayer.SqlData.Account.AccountIndsamler.IndsamlerTypeEnum.Indsamler;
				default:
					throw new Exception($"Cannot convert {indsamlerType}");
			}
		}
	}
}
