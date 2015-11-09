using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseSquash = DataLayer.MongoData.Option.Options.Logic.Squash;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
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

		protected override bool ExecuteOption()
		{
			DataLayer.MongoData.Progress progress;
			DatabaseContact contact = GetContactToSquash(out progress);

			if (contact == null)
			{
				return false;
			}

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

			Dictionary<Guid, List<DatabaseContactChange>> changesByProviderId = GetChangesByProviderId(contactChanges);

			List<ModifiedField> changedFields = new List<ModifiedField>();

			CollectModifiedFieldsForAllProviders(contactChanges, columnNames, changesByProviderId, changedFields);

			bool contactChanged = UpdateContactFieldsIfNeeded(contact, columnNames, changedFields);

			if (contactChanged == true)
			{
				contact.Update(SqlConnection);
				progress.UpdateLastProgressDateToNow(Connection);
			}

			return true;
		}

		private static void CollectModifiedFieldsForAllProviders(List<DatabaseContactChange> contactChanges, List<string> columnNames, Dictionary<Guid, List<DatabaseContactChange>> changesByProviderId, List<ModifiedField> modifiedFields)
		{
			foreach (Guid providerId in changesByProviderId.Keys)
			{
				DatabaseContactChange contactChangeLast = null;
				foreach (DatabaseContactChange contactChange in changesByProviderId[providerId])
				{
					DateTime modifiedOn = contactChange.ModifiedOn;

					if (contactChangeLast == null)
					{
						CollectAllFieldsAsModifiedField(columnNames, modifiedFields, contactChange, modifiedOn);
					}
					else
					{
						CollectActualChangesFromContactChanges(columnNames, modifiedFields, contactChangeLast, contactChange, modifiedOn);
					}

					contactChangeLast = contactChange;
				}
			}
		}

		private static bool UpdateContactFieldsIfNeeded(DatabaseContact contact, List<string> columnNames, List<ModifiedField> changedFields)
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
				object existingValue = ReflectionHelper.GetValue(contact, columnName);

				if (newValue.Equals(existingValue) == false)
				{
					ReflectionHelper.SetValue(contact, columnName, newValue);
					contactChanged = true;
				}
			}

			return contactChanged;
		}

		private static void CollectAllFieldsAsModifiedField(List<string> columnNames, List<ModifiedField> modifiedFields, DatabaseContactChange contactChange, DateTime modifiedOn)
		{
			foreach (string columnName in columnNames)
			{
				object value = ReflectionHelper.GetValue(contactChange, columnName);

				ModifiedField modifiedField = new ModifiedField()
				{
					ModifiedOn = modifiedOn,
					Name = columnName,
					Value = value,
				};

				modifiedFields.Add(modifiedField);
			}
		}

		private static void CollectActualChangesFromContactChanges(List<string> columnNames, List<ModifiedField> modifiedFields, DatabaseContactChange contactChangeLast, DatabaseContactChange contactChange, DateTime modifiedOn)
		{
			foreach (string columnName in columnNames)
			{
				object lastValue = ReflectionHelper.GetValue(contactChangeLast, columnName);
				object currentValue = ReflectionHelper.GetValue(contactChange, columnName);

				if (lastValue.Equals(currentValue) == false)
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

		private static Dictionary<Guid, List<DatabaseContactChange>> GetChangesByProviderId(List<DatabaseContactChange> contactChanges)
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

		private DatabaseContact GetContactToSquash(out DataLayer.MongoData.Progress progress)
		{
			progress = DataLayer.MongoData.Progress.ReadNext(Connection, "Contact");

			if (progress == null)
			{
				return null;
			}

			Guid contactId = progress.TargetId;

			DatabaseContact contact = DatabaseContact.Read(SqlConnection, contactId);

			return contact;
		}
	}
}
