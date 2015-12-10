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
		public StateEnum State { get; private set; }

		public Guid address1_addressid;
		public DateTime createdon;
		public DateTime modifiedon;
		public Guid contactid;
		public Guid address3_addressid;
		public string lastname;
		public string firstname;
		public Guid address2_addressid;
		public DateTime? birthdate;

		public string address1_line1;
		public string address1_line2;
		public string address1_city;
		public string address1_postalcode;
		public string emailaddress1;
		public string mobilephone;
		public string telephone1;

		public string new_cprnr;

		public int new_kkadminmedlemsnr;
		public string new_storkredsnavn;
		public int new_storkredsnr;
		public string new_kkadminsoegenavn;
		public DateTime? new_gavebrevudloebsdato;
		public string new_titel;
		public bool new_hargavebrev;
		public bool new_kkadminstatus;

		public string notat;

		public List<Group> Groups = new List<Group>();
		private static string _groupRelationshipName = "new_group_contact";

		private static readonly ColumnSet ColumnSetContact = new ColumnSet(
			"contactid",

			"new_cprnr",

			"address1_line1",
			"address1_line2",
			"address1_city",
			"address1_postalcode",
			"emailaddress1",
			"mobilephone",
			"telephone1",
			"new_kkadminmedlemsnr",
			"birthdate",

			"new_gavebrevudloebsdato",
			"new_storkredsnavn",
			"new_storkredsnr",
			"new_kkadminsoegenavn",
			"new_titel",
			"new_hargavebrev",
			"new_kkadminstatus",

			"address1_addressid",
			"address2_addressid",
			"address3_addressid",

			"createdon",
			"modifiedon",

			"firstname",
			"lastname",
			"statecode");

		private static readonly ColumnSet ColumnSetContactCrmGenerated = new ColumnSet("address1_addressid", "createdon", "modifiedon", "contactid", "address3_addressid", "address2_addressid", "statecode");

		private static Contact EntityToContact(Entity entity)
		{
			Contact contact = new Contact();

			contact.State = (StateEnum)((OptionSetValue)entity.Attributes["statecode"]).Value;

			foreach (string key in entity.Attributes.Keys)
			{
				Utilities.ReflectionHelper.SetValue(contact, key, entity.Attributes[key]);
			}

			return contact;
		}

		private void ReadGroups(DynamicsCrmConnection connection, Entity contactEntity)
		{
			Relationship relationShip = new Relationship(_groupRelationshipName);
			IEnumerable<Entity> relatedEntities = contactEntity.GetRelatedEntities(connection.Context, relationShip);

			Groups = new List<Group>();

			foreach (Entity relatedEntity in relatedEntities)
			{
				Group group = new Group(relatedEntity);
				Groups.Add(group);
			}
		}

		private void SynchronizeGroupsInCrm(DynamicsCrmConnection connection, Entity contactEntity)
		{
			Relationship relationShip = new Relationship(_groupRelationshipName);
			IEnumerable<Entity> relatedEntities = contactEntity.GetRelatedEntities(connection.Context, relationShip);

			Dictionary<Guid, Entity> entitiesByKey = relatedEntities.ToDictionary(entity => entity.GetAttributeValue<Guid>("new_groupid"));

			List<Group> groupsLocalButNotInCrm = Groups.Where(group => entitiesByKey.ContainsKey(group.GroupId) == false).ToList();

			List<Entity> groupInCrmButNotLocal = entitiesByKey.
				Where(entityAndKey => Groups.
					Any(group => group.GroupId == entityAndKey.Key) == false).
				Select(entityAndKey => entityAndKey.Value).ToList();

			AddGroupRelationShip(connection, groupsLocalButNotInCrm);

			RemoveGroupRelationShip(connection, groupInCrmButNotLocal);
		}

		private void AddGroupRelationShip(DynamicsCrmConnection connection, List<Group> groupsToAdd)
		{
			if (groupsToAdd.Any() == false)
			{
				return;
			}

			Relationship relationShip = new Relationship(_groupRelationshipName);
			EntityReferenceCollection entityReferenceCollection = new EntityReferenceCollection();

			foreach (Group group in groupsToAdd)
			{
				EntityReference entityReference = new EntityReference("new_group", group.GroupId);
				entityReferenceCollection.Add(entityReference);
			}

			connection.Service.Associate("contact", contactid, relationShip, entityReferenceCollection);
		}

		private void RemoveGroupRelationShip(DynamicsCrmConnection connection, List<Entity> groupsToRemove)
		{
			if (groupsToRemove.Any() == false)
			{
				return;
			}

			Relationship relationShip = new Relationship(_groupRelationshipName);
			EntityReferenceCollection entityReferenceCollection = new EntityReferenceCollection();

			foreach (Entity groupEntity in groupsToRemove)
			{
				Guid groupId = groupEntity.GetAttributeValue<Guid>("new_groupid");

				EntityReference entityReference = new EntityReference("new_group", groupId);
				entityReferenceCollection.Add(entityReference);
			}

			connection.Service.Disassociate("contact", contactid, relationShip, entityReferenceCollection);
		}

		private void ReadCrmGeneratedFields(Entity entity)
		{
			contactid = (Guid)entity.Attributes["contactid"];

			createdon = (DateTime)entity.Attributes["createdon"];

			modifiedon = (DateTime)entity.Attributes["modifiedon"];

			address1_addressid = (Guid)entity.Attributes["address1_addressid"];
			address2_addressid = (Guid)entity.Attributes["address2_addressid"];
			address3_addressid = (Guid)entity.Attributes["address3_addressid"];
		}

		private CrmEntity GetContactAsEntity(bool includeContactId)
		{
			CrmEntity crmEntity = new CrmEntity("contact");

			if (includeContactId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("contactid", contactid));
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("firstname", firstname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("lastname", lastname));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_hargavebrev", new_hargavebrev));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminstatus", new_kkadminstatus));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_gavebrevudloebsdato", new_gavebrevudloebsdato));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_storkredsnavn", new_storkredsnavn));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_storkredsnr", new_storkredsnr));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminsoegenavn", new_kkadminsoegenavn));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminmedlemsnr", new_kkadminmedlemsnr));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_titel", new_titel));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line1", address1_line1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line2", address1_line2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_city", address1_city));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_postalcode", address1_postalcode));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("emailaddress1", emailaddress1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("mobilephone", mobilephone));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("telephone1", telephone1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("birthdate", birthdate));

			return crmEntity;
		}

		private static readonly DateTime _minimumSearchDate = new DateTime(1900, 1, 1);
		public static List<Contact> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate)
		{
			if (lastSearchDate <= _minimumSearchDate)
			{
				lastSearchDate = _minimumSearchDate;
			}

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

			attributeNames = entity.Attributes.Select(attribute => $"{attribute.Value.GetType().Name} {attribute.Key}").ToList();

			return attributeNames;
		}

		public static Contact Read(DynamicsCrmConnection connection, Guid contactid)
		{
			Entity contactEntity = connection.Service.Retrieve("contact", contactid, ColumnSetContact);

			Contact contact = EntityToContact(contactEntity);
			contact.ReadGroups(connection, contactEntity);

			return contact;
		}

		public void Delete(DynamicsCrmConnection connection)
		{
			connection.Service.Delete("contact", contactid);
		}

		public void Insert(DynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetContactAsEntity(false);

			contactid = connection.Service.Create(crmEntity);

			Entity contactEntity = connection.Service.Retrieve("contact", contactid, ColumnSetContactCrmGenerated);
			ReadCrmGeneratedFields(contactEntity);

			SynchronizeGroupsInCrm(connection, contactEntity);
		}

		public void Update(DynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetContactAsEntity(true);

			connection.Service.Update(crmEntity);

			Entity contactEntity = connection.Service.Retrieve("contact", contactid, ColumnSetContactCrmGenerated);
			ReadCrmGeneratedFields(contactEntity);

			SynchronizeGroupsInCrm(connection, contactEntity);
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

		public void SetActive(DynamicsCrmConnection connection, bool isActive)
		{
			SetStateRequest setStateRequest = new SetStateRequest()
			{
				EntityMoniker = new EntityReference
				{
					Id = contactid,
					LogicalName = "contact",
				},
				//State = new OptionSetValue(1),
				//Status = new OptionSetValue(2),
			};

			if (isActive == true)
			{
				setStateRequest.State = new OptionSetValue((int)StateEnum.Active);
				setStateRequest.Status = new OptionSetValue((int)StatusEnum.Active);
			}
			else
			{
				setStateRequest.State = new OptionSetValue((int)StateEnum.Inactive);
				setStateRequest.Status = new OptionSetValue((int)StatusEnum.Inactive);
			}

			connection.Service.Execute(setStateRequest);
		}
	}
}
