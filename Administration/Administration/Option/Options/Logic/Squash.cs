using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseSquash = DataLayer.MongoData.Option.Options.Logic.Squash;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using Utilities;
using Administration.Option.Options.Logic.SquashData;

namespace Administration.Option.Options.Logic
{
	public class Squash : AbstractDataOptionBase
	{
		private DatabaseSquash _databaseSquash;

		public Squash(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSquash = (DatabaseSquash)databaseOption;
		}

		public static List<Squash> Find(MongoConnection connection)
		{
			List<DatabaseSquash> options = DatabaseOptionBase.ReadAllowed<DatabaseSquash>(connection);

			return options.Select(option => new Squash(connection, option)).ToList();
		}

		protected override bool ExecuteOption()
		{
			bool contactSquashed = SquashContact();
			bool accountSquashed = SquashAccount();

			return contactSquashed && accountSquashed;
		}

		private bool SquashContact()
		{
			DataLayer.MongoData.Progress progress;
			DatabaseContact contact = GetContactToSquash(out progress);

			if (contact == null)
			{
				return false;
			}

			bool contactChanged = SquashContact(contact);

			if (contactChanged == true)
			{
				contact.Update(SqlConnection);
				progress.UpdateAndSetLastProgressDateToNow(Connection);
			}

			return true;
		}

		private bool SquashAccount()
		{
			DataLayer.MongoData.Progress progress;
			DatabaseAccount account = GetAccountToSquash(out progress);

			if (account == null)
			{
				return false;
			}

			bool accountChanged = SquashAccount(account);

			if (accountChanged == true)
			{
				account.Update(SqlConnection);
				progress.UpdateAndSetLastProgressDateToNow(Connection);
			}

			return true;
		}

		public bool SquashContact(DatabaseContact contact)
		{
			List<DatabaseContactChange> contactChanges = DatabaseContactChange.Read(SqlConnection, contact.Id, DatabaseContactChange.IdType.ContactId);

			contactChanges = contactChanges.OrderBy(contactChange => contactChange.ModifiedOn).ToList();

			List<string> exclusionList = new List<string>()
			{
				"Id",
				"ModifiedOn",
				"CreatedOn",
				"ExternalContactId",
				"ChangeProviderId",
				"ContactId"
			};

			List<string> columnNames = ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseContactChange), exclusionList);

			Dictionary<Guid, List<DatabaseContactChange>> changesByProviderId = GetContactChangesByProviderId(contactChanges);

			List<ModifiedField> changedFields = new List<ModifiedField>();

			CollectModifiedFieldsForAllProviders(columnNames, changesByProviderId, changedFields, databaseContactChange => databaseContactChange.ModifiedOn);

			bool contactChanged = UpdateFieldsIfNeeded(contact, columnNames, changedFields);

			return contactChanged;
		}

		public bool SquashAccount(DatabaseAccount account)
		{
			List<DatabaseAccountChange> accountChanges = DatabaseAccountChange.Read(SqlConnection, account.Id, DatabaseAccountChange.IdType.AccountId);

			accountChanges = accountChanges.OrderBy(contactChange => contactChange.ModifiedOn).ToList();

			List<string> exclusionList = new List<string>()
			{
				"Id",
				"ModifiedOn",
				"CreatedOn",
				"ExternalAccountId",
				"ChangeProviderId",
				"AccountId"
			};

			List<string> columnNames = ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseAccountChange), exclusionList);

			Dictionary<Guid, List<DatabaseAccountChange>> changesByProviderId = GetAccountChangesByProviderId(accountChanges);

			List<ModifiedField> changedFields = new List<ModifiedField>();

			CollectModifiedFieldsForAllProviders(columnNames, changesByProviderId, changedFields, databaseContactChange => databaseContactChange.ModifiedOn);

			bool contactChanged = UpdateFieldsIfNeeded(account, columnNames, changedFields);

			return contactChanged;
		}

		private static void CollectModifiedFieldsForAllProviders<DatabaseType>(List<string> columnNames, Dictionary<Guid, List<DatabaseType>> changesByProviderId, List<ModifiedField> modifiedFields, Func<DatabaseType, DateTime> GetModifiedOn)
		{
			foreach (Guid providerId in changesByProviderId.Keys)
			{
				DatabaseType databaseChangeLast = default(DatabaseType);
				foreach (DatabaseType databaseChange in changesByProviderId[providerId])
				{
					DateTime modifiedOn = GetModifiedOn(databaseChange);

					if (databaseChangeLast == null)
					{
						CollectAllFieldsAsModifiedField(columnNames, modifiedFields, databaseChange, modifiedOn);
					}
					else
					{
						CollectActualChangesFromDatabaseChanges(columnNames, modifiedFields, databaseChangeLast, databaseChange, modifiedOn);
					}

					databaseChangeLast = databaseChange;
				}
			}
		}

		private static bool UpdateFieldsIfNeeded(object databaseObject, List<string> columnNames, List<ModifiedField> changedFields)
		{
			bool contactChanged = false;

			foreach (string columnName in columnNames)
			{
				List<ModifiedField> changesToCurrentColumn = changedFields.Where(field => field.Name == columnName).ToList();

				if (changesToCurrentColumn.Any() == false)
				{
					continue;
				}

				changesToCurrentColumn = changesToCurrentColumn.OrderByDescending(change => change.ModifiedOn).ToList();

				ModifiedField latestChange = changesToCurrentColumn.First();

				object newValue = latestChange.Value;
				object existingValue = ReflectionHelper.GetValue(databaseObject, columnName);

				if
				(
					(newValue == null && existingValue != null) ||
					(newValue != null && existingValue == null) ||
					(newValue != null && existingValue != null && newValue.Equals(existingValue) == false)

				)
				{
					ReflectionHelper.SetValue(databaseObject, columnName, newValue);
					contactChanged = true;
				}
			}

			return contactChanged;
		}

		private static void CollectAllFieldsAsModifiedField(List<string> columnNames, List<ModifiedField> modifiedFields, object databaseChange, DateTime modifiedOn)
		{
			foreach (string columnName in columnNames)
			{
				object value = ReflectionHelper.GetValue(databaseChange, columnName);

				ModifiedField modifiedField = new ModifiedField()
				{
					ModifiedOn = modifiedOn,
					Name = columnName,
					Value = value,
				};

				modifiedFields.Add(modifiedField);
			}
		}

		private static void CollectActualChangesFromDatabaseChanges(List<string> columnNames, List<ModifiedField> modifiedFields, object databaseChangeLast, object databaseChange, DateTime modifiedOn)
		{
			foreach (string columnName in columnNames)
			{
				object lastValue = ReflectionHelper.GetValue(databaseChangeLast, columnName);
				object currentValue = ReflectionHelper.GetValue(databaseChange, columnName);

				if
				(
					(lastValue == null && currentValue != null) ||
					(lastValue != null && currentValue == null) ||
					(lastValue != null && currentValue != null && lastValue.Equals(currentValue) == false)
				)
				{
					ModifiedField modifiedField = new ModifiedField()
					{
						ModifiedOn = modifiedOn,
						Name = columnName,
						Value = currentValue,
					};

					modifiedFields.Add(modifiedField);
				}
			}
		}

		private static Dictionary<Guid, List<DatabaseContactChange>> GetContactChangesByProviderId(List<DatabaseContactChange> contactChanges)
		{
			Dictionary<Guid, List<DatabaseContactChange>> changesByProviderId = new Dictionary<Guid, List<DatabaseContactChange>>();

			foreach (DatabaseContactChange contactChange in contactChanges)
			{
				Guid providerId = contactChange.ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<DatabaseContactChange>());
				}

				changesByProviderId[providerId].Add(contactChange);
			}

			return changesByProviderId;
		}

		private static Dictionary<Guid, List<DatabaseAccountChange>> GetAccountChangesByProviderId(List<DatabaseAccountChange> accountChanges)
		{
			Dictionary<Guid, List<DatabaseAccountChange>> changesByProviderId = new Dictionary<Guid, List<DatabaseAccountChange>>();

			foreach (DatabaseAccountChange accountChange in accountChanges)
			{
				Guid providerId = accountChange.ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<DatabaseAccountChange>());
				}

				changesByProviderId[providerId].Add(accountChange);
			}

			return changesByProviderId;
		}

		private DatabaseContact GetContactToSquash(out DataLayer.MongoData.Progress progress)
		{
			progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressContact);

			if (progress == null)
			{
				return null;
			}

			Guid contactId = progress.TargetId;

			DatabaseContact contact = DatabaseContact.Read(SqlConnection, contactId);

			return contact;
		}

		private DatabaseAccount GetAccountToSquash(out DataLayer.MongoData.Progress progress)
		{
			progress = DataLayer.MongoData.Progress.ReadNext(Connection, MaintainProgress.ProgressAccount);

			if (progress == null)
			{
				return null;
			}

			Guid accountId = progress.TargetId;

			DatabaseAccount account = DatabaseAccount.Read(SqlConnection, accountId);

			return account;
		}
	}
}
