using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace SystemInterface.Dynamics.Crm
{
	public class Contact
	{
		public Guid ContactId;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;
		public StateEnum State { get; private set; }

		private static readonly ColumnSet ColumnSetContact = new ColumnSet("contactid", "createdon", "modifiedon", "firstname", "lastname", "statecode");

		private static Contact EntityToContact(Entity entity)
		{
			return new Contact
			{
				ContactId = (Guid)entity.Attributes["contactid"],
				CreatedOn = (DateTime)entity.Attributes["createdon"],
				ModifiedOn = (DateTime)entity.Attributes["modifiedon"],
				Firstname = entity.Attributes["firstname"].ToString(),
				Lastname = entity.Attributes["lastname"].ToString(),
				State = (StateEnum)((OptionSetValue)entity.Attributes["statecode"]).Value,
			};
		}

		private CrmEntity GetContactAsEntity(bool includeContactId)
		{
			CrmEntity crmEntity = new CrmEntity("contact");
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("firstname", Firstname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("lastname", Lastname));

			if (includeContactId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("contactid", ContactId));
			}

			return crmEntity;
		}

		public static List<Contact> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate)
		{
			ConditionExpression modifiedOnExpression = new ConditionExpression
			{
				AttributeName = "modifiedon",
				Operator = ConditionOperator.GreaterEqual
			};
			modifiedOnExpression.Values.Add(lastSearchDate);

			FilterExpression filterExpression = new FilterExpression();
			filterExpression.Conditions.Add(modifiedOnExpression);

			QueryExpression query = new QueryExpression("contact")
			{
				ColumnSet = ColumnSetContact
			};
			query.Criteria.AddFilter(filterExpression);

			EntityCollection entityCollection = connection.Service.RetrieveMultiple(query);

			List<Contact> contacts = entityCollection.Entities.Select(EntityToContact).ToList();

			return contacts;
		}

		public static List<string> GetAllAttributeNames(DynamicsCrmConnection connection, Guid contactId)
		{
			List<string> attributeNames = new List<string>();

			ColumnSet columnsAll = new ColumnSet(true);

			Entity entity = connection.Service.Retrieve("contact", contactId, columnsAll);

			attributeNames = entity.Attributes.Select(attribute => attribute.Key).ToList();

			return attributeNames;
		}

		public static Contact Read(DynamicsCrmConnection connection, Guid contactid)
		{
			Entity contactEntity = connection.Service.Retrieve("contact", contactid, ColumnSetContact);

			Contact contact = EntityToContact(contactEntity);

			return contact;
		}

		public void Delete(DynamicsCrmConnection connection)
		{
			connection.Service.Delete("contact", ContactId);
		}

		public void Insert(DynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetContactAsEntity(false);

			ContactId = connection.Service.Create(crmEntity);
		}

		public void Update(DynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetContactAsEntity(true);

			connection.Service.Update(crmEntity);
		}

		public enum StateEnum
		{
			Active = 0,
			Inactive = 1,
		}

		public enum StatusEnum
		{
			Active = 1,
			Inactive = 2,
		}
	}
}
