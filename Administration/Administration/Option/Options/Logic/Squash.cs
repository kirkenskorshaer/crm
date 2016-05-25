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
using DatabaseAccountGroup = DataLayer.SqlData.Group.AccountGroup;
using DatabaseAccountContact = DataLayer.SqlData.Account.AccountContact;
using DatabaseAccountIndsamler = DataLayer.SqlData.Account.AccountIndsamler;
using DatabaseAccountChangeGroup = DataLayer.SqlData.Group.AccountChangeGroup;
using DatabaseAccountChangeContact = DataLayer.SqlData.Account.AccountChangeContact;
using DatabaseAccountChangeIndsamler = DataLayer.SqlData.Account.AccountChangeIndsamler;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using Utilities;
using Administration.Option.Options.Logic.SquashData;
using DataLayer.SqlData;
using Utilities.Comparer;
using DataLayer.SqlData.Annotation;
using DataLayer.SqlData.Contact;
using System.Data.SqlClient;

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

			contactChanges = contactChanges.OrderBy(contactChange => contactChange.modifiedon).ToList();

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
				{ typeof(DatabaseContactChangeGroup), new ReferenceGetAndSet() {GetReferences = GroupFromContactId, SetReferences = (guids) => SetGroupsOnContact(contact, guids)} },
			};

			CollectModifiedReferencesForAllProviders(changesByProviderId, changedReferences, referenceTypeAndGetReference);

			bool contactFieldChanged = UpdateFieldsIfNeeded(contact, columnNames, changedFields);

			bool contactReferenceChanged = UpdateReferencesIfNeeded(contact, referenceGetAndSets, changedReferences);

			SquashAnnotations(contact);

			return contactFieldChanged;
		}

		public bool SquashAccount(DatabaseAccount account)
		{
			List<IModifiedIdData> accountChanges = DatabaseAccountChange.Read(SqlConnection, account.Id, DatabaseAccountChange.IdType.AccountId).Select(data => (IModifiedIdData)data).ToList();

			accountChanges = accountChanges.OrderBy(contactChange => contactChange.modifiedon).ToList();

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
			bool accountChanged = UpdateFieldsIfNeeded(account, columnNames, changedFields);

			List<ModifiedReference> changedReferences = new List<ModifiedReference>();

			Dictionary<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference = new Dictionary<Type, Func<Guid, List<Guid>>>()
			{
				{ typeof(DatabaseAccountChangeGroup), GroupFromAccountChangeId },
				{ typeof(DatabaseAccountChangeContact), ContactFromAccountChangeId },
				{ typeof(DatabaseAccountChangeIndsamler), IndsamlerFromAccountChangeId },
			};

			CollectModifiedReferencesForAllProviders(changesByProviderId, changedReferences, referenceTypeAndGetReference);

			Dictionary<Type, ReferenceGetAndSet> referenceGetAndSets = new Dictionary<Type, ReferenceGetAndSet>()
			{
				{ typeof(DatabaseAccountChangeGroup), new ReferenceGetAndSet() {GetReferences = GroupFromAccountId, SetReferences = (guids) => SetGroupsOnAccount(account, guids)} },
				{ typeof(DatabaseAccountChangeContact), new ReferenceGetAndSet() {GetReferences = ContactFromAccountId, SetReferences = (guids) => SetContactsOnAccount(account, guids)} },
			};

			foreach (SystemInterface.Dynamics.Crm.IndsamlerDefinition definition in SystemInterface.Dynamics.Crm.Account.IndsamlerRelationshipDefinitions)
			{
				DatabaseAccountIndsamler.IndsamlerTypeEnum databaseIndsamlerType = Conversion.Account.Convert(definition.IndsamlerType);
				referenceGetAndSets.Add(typeof(DatabaseAccountChangeIndsamler), new ReferenceGetAndSet() { GetReferences = IndsamlerFromAccountId, SetReferences = (guids) => SetIndsamlereOnAccount(account, guids, databaseIndsamlerType, definition.Aar) });
			}

			bool accountReferenceChanged = UpdateReferencesIfNeeded(account, referenceGetAndSets, changedReferences);

			SquashAnnotations(account);

			return accountChanged;
		}

		public void SquashAnnotations(DatabaseContact contact)
		{
			List<ContactAnnotation> contactAnnotationsOnContact = ContactAnnotation.ReadByContactId(SqlConnection, contact.Id);

			List<ContactAnnotation> squashedAnnotations = new List<ContactAnnotation>();
			foreach (ContactAnnotation contactAnnotation in contactAnnotationsOnContact)
			{
				ContactAnnotation squashedAnnotation = SquashAnnotation(contactAnnotation);
				if (squashedAnnotation != null)
				{
					squashedAnnotations.Add(squashedAnnotation);
				}
			}

			contact.RemoveAnnotationsNotInList(SqlConnection, squashedAnnotations.Select(annotation => annotation.Id).ToList());
		}

		public void SquashAnnotations(DatabaseAccount account)
		{
			List<AccountAnnotation> accountAnnotationsOnAccount = AccountAnnotation.ReadByAccountId(SqlConnection, account.Id);

			List<AccountAnnotation> squashedAnnotations = new List<AccountAnnotation>();
			foreach (AccountAnnotation accountAnnotation in accountAnnotationsOnAccount)
			{
				AccountAnnotation squashedAnnotation = SquashAnnotation(accountAnnotation);
				if (squashedAnnotation != null)
				{
					squashedAnnotations.Add(squashedAnnotation);
				}
			}

			account.RemoveAnnotationsNotInList(SqlConnection, squashedAnnotations.Select(annotation => annotation.Id).ToList());
		}

		public ContactAnnotation SquashAnnotation(ContactAnnotation annotation)
		{
			List<IModifiedIdData> contactChangeAnnotations = ContactChangeAnnotation.ReadByContactAnnotationId(SqlConnection, annotation.Id).Select(data => (IModifiedIdData)data).ToList();

			contactChangeAnnotations = contactChangeAnnotations.OrderBy(contactChange => contactChange.modifiedon).ToList();

			List<string> exclusionList = new List<string>()
			{
				"Id",
				"ContactChangeId",
				"ContactAnnotationId",
			};

			List<string> columnNames = ReflectionHelper.GetFieldsAndProperties(typeof(ContactChangeAnnotation), exclusionList);

			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = GetContactChangeAnnotationsByProviderId(SqlConnection, contactChangeAnnotations);
			List<ModifiedField> changedFields = new List<ModifiedField>();
			CollectModifiedFieldsForAllProviders(columnNames, changesByProviderId, changedFields);

			bool annotationFieldChanged = UpdateFieldsIfNeeded(annotation, columnNames, changedFields);

			if (annotationFieldChanged)
			{
				annotation.Update(SqlConnection);
			}

			return annotation;
		}

		public AccountAnnotation SquashAnnotation(AccountAnnotation annotation)
		{
			List<IModifiedIdData> accountChangeAnnotations = AccountChangeAnnotation.ReadByAccountAnnotationId(SqlConnection, annotation.Id).Select(data => (IModifiedIdData)data).ToList();

			accountChangeAnnotations = accountChangeAnnotations.OrderBy(accountChange => accountChange.modifiedon).ToList();

			List<string> exclusionList = new List<string>()
			{
				"Id",
				"AccountChangeId",
				"AccountAnnotationId",
			};

			List<string> columnNames = ReflectionHelper.GetFieldsAndProperties(typeof(AccountChangeAnnotation), exclusionList);

			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = GetAccountChangeAnnotationsByProviderId(SqlConnection, accountChangeAnnotations);
			List<ModifiedField> changedFields = new List<ModifiedField>();
			CollectModifiedFieldsForAllProviders(columnNames, changesByProviderId, changedFields);

			bool annotationFieldChanged = UpdateFieldsIfNeeded(annotation, columnNames, changedFields);

			if (annotationFieldChanged)
			{
				annotation.Update(SqlConnection);
			}

			return annotation;
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

		private List<Guid> GroupFromAccountId(Guid id)
		{
			return DatabaseAccountGroup.ReadFromAccountId(SqlConnection, id).Select(accountGroup => accountGroup.GroupId).ToList();
		}

		private List<Guid> ContactFromAccountId(Guid id)
		{
			return DatabaseAccountContact.ReadFromAccountId(SqlConnection, id).Select(accountContact => accountContact.ContactId).ToList();
		}

		private List<Guid> IndsamlerFromAccountId(Guid id)
		{
			return DatabaseAccountIndsamler.ReadFromAccountId(SqlConnection, id).Select(accountContact => accountContact.ContactId).ToList();
		}

		private void SetGroupsOnContact(DatabaseContact databaseContact, List<Guid> groupIds)
		{
			databaseContact.SynchronizeGroups(SqlConnection, groupIds);
		}

		private void SetGroupsOnAccount(DatabaseAccount databaseAccount, List<Guid> groupIds)
		{
			databaseAccount.SynchronizeGroups(SqlConnection, groupIds);
		}

		private void SetContactsOnAccount(DatabaseAccount databaseAccount, List<Guid> contactIds)
		{
			databaseAccount.SynchronizeContacts(SqlConnection, contactIds);
		}

		private void SetIndsamlereOnAccount(DatabaseAccount databaseAccount, List<Guid> indsamlerIds, DatabaseAccountIndsamler.IndsamlerTypeEnum indsamlerType, int aar)
		{
			databaseAccount.SynchronizeIndsamlere(SqlConnection, indsamlerIds, indsamlerType, aar);
		}

		private void CollectModifiedFieldsForAllProviders(List<string> columnNames, Dictionary<Guid, List<IModifiedIdData>> changesByProviderId, List<ModifiedField> modifiedFields)
		{
			foreach (Guid providerId in changesByProviderId.Keys)
			{
				IModifiedIdData databaseChangeLast = default(IModifiedIdData);

				List<IModifiedIdData> changeSetsForGivenProvider = changesByProviderId[providerId];

				List<string> definedColumnsOnCurrentProvider = FindDefinedColumns(columnNames, providerId, changeSetsForGivenProvider);

				foreach (IModifiedIdData databaseChange in changeSetsForGivenProvider)
				{
					DateTime modifiedOn = databaseChange.modifiedon;

					if (databaseChangeLast == null)
					{
						CollectAllFieldsAsModifiedField(definedColumnsOnCurrentProvider, modifiedFields, databaseChange, modifiedOn);
					}
					else
					{
						CollectActualChangesFromDatabaseChanges(definedColumnsOnCurrentProvider, modifiedFields, databaseChangeLast, databaseChange, modifiedOn);
					}

					databaseChangeLast = databaseChange;
				}
			}
		}

		private List<string> FindDefinedColumns(List<string> columnNames, Guid changeProviderId, List<IModifiedIdData> changeSets)
		{
			List<string> definedColumns = new List<string>();

			foreach (string columnName in columnNames)
			{
				foreach (IModifiedIdData changeSet in changeSets)
				{
					object currentValue = ReflectionHelper.GetValue(changeSet, columnName);
					if (currentValue != null)
					{
						definedColumns.Add(columnName);
						break;
					}
				}
			}

			return definedColumns;
		}

		private void CollectModifiedReferencesForAllProviders(Dictionary<Guid, List<IModifiedIdData>> changesByProviderId, List<ModifiedReference> modifiedReferences, Dictionary<Type, Func<Guid, List<Guid>>> ReferenceTypeAndGetReferenceCollection)
		{
			foreach (Guid providerId in changesByProviderId.Keys)
			{
				foreach (KeyValuePair<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference in ReferenceTypeAndGetReferenceCollection)
				{
					IModifiedIdData databaseChangeLast = default(IModifiedIdData);
					List<Guid> databaseLastReferences = null;

					foreach (IModifiedIdData databaseChange in changesByProviderId[providerId])
					{
						DateTime modifiedOn = databaseChange.modifiedon;

						if (databaseChangeLast == null)
						{
							databaseLastReferences = CollectAllReferencesAsModifiedReference(modifiedReferences, modifiedOn, databaseChange.Id, referenceTypeAndGetReference);
						}
						else
						{
							databaseLastReferences = CollectActualReferencesChangesAsModifiedReference(modifiedReferences, modifiedOn, databaseChange.Id, databaseLastReferences, referenceTypeAndGetReference);
						}
						databaseChangeLast = databaseChange;
					}
				}
			}
		}

		private List<Guid> CollectAllReferencesAsModifiedReference(List<ModifiedReference> modifiedReferences, DateTime modifiedOn, Guid databaseId, KeyValuePair<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference)
		{
			List<Guid> currentValue = referenceTypeAndGetReference.Value(databaseId).ToList();

			if (currentValue.Any())
			{
				ModifiedReference modifiedReference = new ModifiedReference()
				{
					RelationType = referenceTypeAndGetReference.Key,
					RelationIdsAdded = currentValue,
					RelationIdsRemoved = new List<Guid>(),
					ModifiedOn = modifiedOn,
				};

				modifiedReferences.Add(modifiedReference);
			}

			return currentValue;
		}

		private List<Guid> CollectActualReferencesChangesAsModifiedReference(List<ModifiedReference> modifiedReferences, DateTime modifiedOn, Guid databaseId, List<Guid> lastValue, KeyValuePair<Type, Func<Guid, List<Guid>>> referenceTypeAndGetReference)
		{
			List<Guid> currentValue = referenceTypeAndGetReference.Value(databaseId).ToList();

			if (ListCompare.ListEquals(currentValue, lastValue) == false)
			{
				ModifiedReference modifiedReference = new ModifiedReference()
				{
					ModifiedOn = modifiedOn,
					RelationType = referenceTypeAndGetReference.Key,
					RelationIdsAdded = currentValue.Except(lastValue).ToList(),
					RelationIdsRemoved = lastValue.Except(currentValue).ToList(),
				};

				modifiedReferences.Add(modifiedReference);
			}

			return currentValue;
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

				modifedReferences = modifedReferences.OrderBy(change => change.ModifiedOn).ToList();

				ModifiedReference finalChange = null;

				foreach (ModifiedReference currentModifiedReference in modifedReferences)
				{
					if (finalChange == null)
					{
						finalChange = new ModifiedReference()
						{
							RelationIdsAdded = currentModifiedReference.RelationIdsAdded,
							RelationIdsRemoved = currentModifiedReference.RelationIdsRemoved,
							RelationType = referenceType,
							ModifiedOn = currentModifiedReference.ModifiedOn,
						};
					}
					else
					{
						finalChange.RelationIdsAdded.AddRange(currentModifiedReference.RelationIdsAdded.Where(relationIdToAdd => finalChange.RelationIdsAdded.Contains(relationIdToAdd) == false));
						finalChange.RelationIdsAdded.RemoveAll(change => currentModifiedReference.RelationIdsRemoved.Contains(change));

						finalChange.RelationIdsRemoved.AddRange(currentModifiedReference.RelationIdsRemoved.Where(relationIdToAdd => finalChange.RelationIdsRemoved.Contains(relationIdToAdd) == false));
						finalChange.RelationIdsRemoved.RemoveAll(change => currentModifiedReference.RelationIdsAdded.Contains(change));
						finalChange.ModifiedOn = currentModifiedReference.ModifiedOn;
					}
				}

				ModifiedReference latestChange = modifedReferences.First();

				List<Guid> newValue = latestChange.RelationIdsAdded;
				List<Guid> existingValue = referenceGetAndSetDictionary[referenceType].GetReferences(databaseObject.Id);

				if (ListCompare.ListEquals(newValue, existingValue) == false)
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

		private static Dictionary<Guid, List<IModifiedIdData>> GetContactChangeAnnotationsByProviderId(SqlConnection sqlConnection, List<IModifiedIdData> contactChangeAnnotations)
		{
			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = new Dictionary<Guid, List<IModifiedIdData>>();

			foreach (ContactChangeAnnotation contactChangeAnnotation in contactChangeAnnotations)
			{
				Guid providerId = DatabaseContactChange.Read(sqlConnection, contactChangeAnnotation.ContactChangeId, DatabaseContactChange.IdType.ContactChangeId).Single().ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<IModifiedIdData>());
				}

				changesByProviderId[providerId].Add(contactChangeAnnotation);
			}

			return changesByProviderId;
		}

		private static Dictionary<Guid, List<IModifiedIdData>> GetAccountChangeAnnotationsByProviderId(SqlConnection sqlConnection, List<IModifiedIdData> accountChangeAnnotations)
		{
			Dictionary<Guid, List<IModifiedIdData>> changesByProviderId = new Dictionary<Guid, List<IModifiedIdData>>();

			foreach (AccountChangeAnnotation accountChangeAnnotation in accountChangeAnnotations)
			{
				Guid providerId = DatabaseAccountChange.Read(sqlConnection, accountChangeAnnotation.AccountChangeId, DatabaseAccountChange.IdType.AccountChangeId).Single().ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<IModifiedIdData>());
				}

				changesByProviderId[providerId].Add(accountChangeAnnotation);
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
