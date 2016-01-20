using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseSquash = DataLayer.MongoData.Option.Options.Logic.Squash;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChangeGroup = DataLayer.SqlData.Group.ContactChangeGroup;
using DatabaseContactGroup = DataLayer.SqlData.Group.ContactGroup;
using DatabaseAccountChangeGroup = DataLayer.SqlData.Group.AccountChangeGroup;
using DatabaseAccountChangeContact = DataLayer.SqlData.Account.AccountChangeContact;
using DatabaseAccountChangeIndsamler = DataLayer.SqlData.Account.AccountChangeIndsamler;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using Utilities;
using Administration.Option.Options.Logic.SquashData;
using DataLayer.SqlData;

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
			List<IModifiedIdData> contactChanges = DatabaseContactChange.Read(SqlConnection, contact.Id, DatabaseContactChange.IdType.ContactId).Select(data => (IModifiedIdData)data).ToList();

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

			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = GetContactChangesByProviderId(contactChanges);
			List<ModifiedField> changedFields = new List<ModifiedField>();
			CollectModifiedFieldsForAllProviders(columnNames, changesByProviderId, changedFields);

			List<ModifiedReference> changedReferences = new List<ModifiedReference>();

			Dictionary<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference = new Dictionary<Type, Func<Guid, List<Guid>>>() { { typeof(DatabaseContactChangeGroup), GroupFromContactChangeId }, };
			Dictionary<Type, ReferenceGetAndSet> referenceGetAndSets = new Dictionary<Type, ReferenceGetAndSet>()
			{
				{ typeof(DatabaseContactChangeGroup), new ReferenceGetAndSet() {GetReferences = GroupFromContactId, SetReferences = SetGroupOnContactGroup} },
			};

			CollectModifiedReferencesForAllProviders(changesByProviderId, changedReferences, referenceTypeAndGetReference);

			bool contactFieldChanged = UpdateFieldsIfNeeded(contact, columnNames, changedFields);

			bool contactReferenceChanged = UpdateReferencesIfNeeded(contact, referenceGetAndSets, changedReferences);

			return contactFieldChanged || contactReferenceChanged;
		}

		public bool SquashAccount(DatabaseAccount account)
		{
			List<IModifiedIdData> accountChanges = DatabaseAccountChange.Read(SqlConnection, account.Id, DatabaseAccountChange.IdType.AccountId).Select(data => (IModifiedIdData)data).ToList();

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

			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = GetAccountChangesByProviderId(accountChanges);

			List<ModifiedField> changedFields = new List<ModifiedField>();
			CollectModifiedFieldsForAllProviders(columnNames, changesByProviderId, changedFields);
			bool contactChanged = UpdateFieldsIfNeeded(account, columnNames, changedFields);

			List<ModifiedReference> changedReferences = new List<ModifiedReference>();

			Dictionary<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference = new Dictionary<Type, Func<Guid, List<Guid>>>()
			{
				{ typeof(DatabaseAccountChangeGroup), GroupFromAccountChangeId },
				{ typeof(DatabaseAccountChangeContact), ContactFromAccountChangeId },
				{ typeof(DatabaseAccountChangeIndsamler), IndsamlerFromAccountChangeId },
			};

			CollectModifiedReferencesForAllProviders(changesByProviderId, changedReferences, referenceTypeAndGetReference);

			return contactChanged;
		}

		private List<Guid> GroupFromAccountChangeId(Guid id)
		{
			return DatabaseAccountChangeGroup.ReadFromAccountChangeId(SqlConnection, id).Select(accountChangeGroup => accountChangeGroup.GroupId).ToList();
		}

		private List<Guid> ContactFromAccountChangeId(Guid id)
		{
			return DatabaseAccountChangeContact.ReadFromAccountChangeId(SqlConnection, id).Select(accountChangeGroup => accountChangeGroup.ContactId).ToList();
		}

		private List<Guid> IndsamlerFromAccountChangeId(Guid id)
		{
			return DatabaseAccountChangeIndsamler.ReadFromAccountChangeId(SqlConnection, id).Select(accountChangeGroup => accountChangeGroup.ContactId).ToList();
		}

		private List<Guid> GroupFromContactChangeId(Guid id)
		{
			return DatabaseContactChangeGroup.ReadFromContactChangeId(SqlConnection, id).Select(contactChangeGroup => contactChangeGroup.GroupId).ToList();
		}

		private List<Guid> GroupFromContactId(Guid id)
		{
			return DatabaseContactGroup.ReadFromContactId(SqlConnection, id).Select(contactGroup => contactGroup.GroupId).ToList();
		}

		private void SetGroupOnContactGroup(List<Guid> groupIds)
		{
			DatabaseContact.SynchronizeGroups(SqlConnection, groupIds);
		}

		private void CollectModifiedFieldsForAllProviders(List<string> columnNames, Dictionary<Guid, List<IModifiedIdData>> changesByProviderId, List<ModifiedField> modifiedFields)
		{
			foreach (Guid providerId in changesByProviderId.Keys)
			{
				IModifiedIdData databaseChangeLast = default(IModifiedIdData);
				foreach (IModifiedIdData databaseChange in changesByProviderId[providerId])
				{
					DateTime modifiedOn = databaseChange.ModifiedOn;

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

		private void CollectModifiedReferencesForAllProviders(Dictionary<Guid, List<IModifiedIdData>> changesByProviderId, List<ModifiedReference> modifiedReferences, Dictionary<Type, Func<Guid, List<Guid>>> ReferenceTypeAndGetReferenceCollection)
		{
			foreach (Guid providerId in changesByProviderId.Keys)
			{
				IModifiedIdData databaseChangeLast = default(IModifiedIdData);
				foreach (IModifiedIdData databaseChange in changesByProviderId[providerId])
				{
					DateTime modifiedOn = databaseChange.ModifiedOn;
					List<Guid> databaseLastReferences = null;

					foreach (KeyValuePair<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference in ReferenceTypeAndGetReferenceCollection)
					{
						if (databaseChangeLast == null)
						{
							CollectAllReferencesAsModifiedReference(modifiedReferences, modifiedOn, databaseChange.Id, referenceTypeAndGetReference);
						}
						else
						{
							databaseLastReferences = CollectActualReferencesChangesAsModifiedReference(modifiedReferences, modifiedOn, databaseChange.Id, databaseLastReferences, referenceTypeAndGetReference);
						}
					}
					databaseChangeLast = databaseChange;
				}
			}
		}

		private void CollectAllReferencesAsModifiedReference(List<ModifiedReference> modifiedReferences, DateTime modifiedOn, Guid databaseId, KeyValuePair<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference)
		{
			ModifiedReference modifiedReference = new ModifiedReference()
			{
				RelationType = referenceTypeAndGetReference.Key,
				RelationIds = referenceTypeAndGetReference.Value(databaseId),
				ModifiedOn = modifiedOn,
			};

			modifiedReferences.Add(modifiedReference);
		}

		private List<Guid> CollectActualReferencesChangesAsModifiedReference(List<ModifiedReference> modifiedReferences, DateTime modifiedOn, Guid databaseId, List<Guid> lastValue, KeyValuePair<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference)
		{
			List<Guid> currentValue = referenceTypeAndGetReference.Value(databaseId).ToList();

			if (GuidListEquals(currentValue, lastValue) == false)
			{
				ModifiedReference modifiedReference = new ModifiedReference()
				{
					ModifiedOn = modifiedOn,
					RelationType = referenceTypeAndGetReference.Key,
					RelationIds = currentValue,
				};

				modifiedReferences.Add(modifiedReference);
			}

			return currentValue;
		}

		private bool GuidListEquals(List<Guid> list1, List<Guid> list2)
		{
			if (list1.Count != list2.Count)
			{
				return false;
			}

			list1.Sort();
			list2.Sort();

			for (int idIndex = 0; idIndex < list1.Count; idIndex++)
			{
				if (list1[idIndex] != list2[idIndex])
				{
					return false;
				}
			}

			return true;
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

		private bool UpdateReferencesIfNeeded(IModifiedIdData databaseObject, Dictionary<Type, ReferenceGetAndSet> referenceGetAndSetDictionary, List<ModifiedReference> changedReferences)
		{
			bool contactChanged = false;

			List<Type> referenceTypes = changedReferences.Select(reference => reference.RelationType).ToList();

			foreach (Type referenceType in referenceTypes)
			{
				List<ModifiedReference> modifedReferences = changedReferences.Where(modifiedReference => modifiedReference.RelationType == referenceType).ToList();

				modifedReferences = modifedReferences.OrderByDescending(change => change.ModifiedOn).ToList();

				ModifiedReference latestChange = modifedReferences.First();

				List<Guid> newValue = latestChange.RelationIds;
				List<Guid> existingValue = referenceGetAndSetDictionary[referenceType].GetReferences(databaseObject.Id);

				if (GuidListEquals(newValue, existingValue) == false)
				{
					referenceGetAndSetDictionary[referenceType].SetReferences(newValue);
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

		private static Dictionary<Guid, List<IModifiedIdData>> GetContactChangesByProviderId(List<IModifiedIdData> contactChanges)
		{
			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = new Dictionary<Guid, List<IModifiedIdData>>();

			foreach (DatabaseContactChange contactChange in contactChanges)
			{
				Guid providerId = contactChange.ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<IModifiedIdData>());
				}

				changesByProviderId[providerId].Add(contactChange);
			}

			return changesByProviderId;
		}

		private static Dictionary<Guid, List<IModifiedIdData>> GetAccountChangesByProviderId(List<IModifiedIdData> accountChanges)
		{
			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = new Dictionary<Guid, List<IModifiedIdData>>();

			foreach (DatabaseAccountChange accountChange in accountChanges)
			{
				Guid providerId = accountChange.ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<IModifiedIdData>());
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
