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
			DatabaseContact contact = GetContactToSquash();

			List<DatabaseContactChange> contactChanges = DatabaseContactChange.Read(SqlConnection, contact.Id, DatabaseContactChange.IdType.ContactId);

			contactChanges = contactChanges.OrderBy(contactChange => contactChange.ModifiedOn).ToList();

			Dictionary<Guid, List<DatabaseContactChange>> changesByProviderId = new Dictionary<Guid, List<DatabaseContactChange>>();

			List<string> exclusionList = new List<string>()
			{
				"Id",
				"ModifiedOn",
				"ExternalContactId",
				"ChangeProviderId",
				"ContactId"
			};

			List<string> columnNames = ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseContactChange), exclusionList);

			foreach (DatabaseContactChange contactChange in contactChanges)
			{
				Guid providerId = contactChange.ChangeProviderId;

				if (changesByProviderId.ContainsKey(providerId) == false)
				{
					changesByProviderId.Add(providerId, new List<DatabaseContactChange>());
				}

				changesByProviderId[providerId].Add(contactChange);
			}

			List<SquashField> changedFields = new List<SquashField>();

			foreach (Guid providerId in changesByProviderId.Keys)
			{
				DatabaseContactChange contactChangeLast = null;
				foreach (DatabaseContactChange contactChange in contactChanges)
				{
					if (contactChangeLast != null)
					{
						DateTime modifiedOn = contactChange.ModifiedOn;

						foreach (string columnName in columnNames)
						{
							object lastValue = ReflectionHelper.GetValue(contactChangeLast, columnName);
							object currentValue = ReflectionHelper.GetValue(contactChange, columnName);

							if (lastValue != currentValue)
							{
								SquashField squashField = new SquashField()
								{
									ModifiedOn = modifiedOn,
									Name = columnName,
									Value = currentValue,
								};

								changedFields.Add(squashField);
							}
						}
					}

					contactChangeLast = contactChange;
				}
			}

			bool contactChanged = false;

			foreach (string columnName in columnNames)
			{
				List<SquashField> changesToCurrentColumn = changedFields.Where(field => field.Name == columnName).ToList();

				changesToCurrentColumn = changesToCurrentColumn.OrderByDescending(change => change.ModifiedOn).ToList();

				SquashField latestChange = changesToCurrentColumn.First();

				object newValue = latestChange.Value;
				object existingValue = ReflectionHelper.GetValue(contact, columnName);

				if(newValue != existingValue)
				{
					ReflectionHelper.SetValue(contact, columnName, newValue);
					contactChanged = true;
				}
            }

			if(contactChanged == true)
			{
				contact.Update(SqlConnection);
			}

			return true;
		}

		private DatabaseContact GetContactToSquash()
		{
			throw new NotImplementedException();
		}
	}
}
