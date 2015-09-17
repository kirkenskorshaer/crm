using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace SystemInterface.Dynamics.Crm
{
	public class Contact
	{
		public Guid ContactId;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;

		private static readonly ColumnSet ColumnSetContact = new ColumnSet("contactid", "createdon", "modifiedon", "firstname", "lastname");

		private static Contact EntityToContact(Entity entity)
		{
			return new Contact
			{
				ContactId = (Guid)entity.Attributes["contactid"],
				CreatedOn = (DateTime)entity.Attributes["createdon"],
				ModifiedOn = (DateTime)entity.Attributes["modifiedon"],
				Firstname = entity.Attributes["firstname"].ToString(),
				Lastname = entity.Attributes["lastname"].ToString(),
			};
		}

		private CrmEntity GetContactAsEntity()
		{
			CrmEntity crmEntity = new CrmEntity("contact");
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("firstname", Firstname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("lastname", Lastname));

			return crmEntity;
		}

		public static List<Contact> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate)
		{
			ConditionExpression modifiedOnExpression = new ConditionExpression
			{
				AttributeName = "modifiedon",
				Operator = ConditionOperator.GreaterThan
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
			CrmEntity crmEntity = GetContactAsEntity();

			ContactId = connection.Service.Create(crmEntity);
		}

		public void Update(DynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetContactAsEntity();

			connection.Service.Update(crmEntity);
		}
	}
}
