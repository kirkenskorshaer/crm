using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace SystemInterface.Dynamics.Crm
{
	public class Contact : AbstractCrm
	{
		public StateEnum State { get; private set; }

		public Guid address1_addressid;
		public DateTime createdon;
		public DateTime modifiedon;
		public Guid address3_addressid;
		public EntityReference modifiedby;
		public string lastname;
		public string middlename;
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

		public EntityReference new_indsamler2016;
		public Guid? indsamler2016 { get { return GetEntityReferenceId(new_indsamler2016); } set { new_indsamler2016 = SetEntityReferenceId(value, "account"); } }

		public string new_oprindelse;
		public string new_oprindelseip;
		public int? new_mailrelaysubscriberid;
		public string new_mailrelaycheck;

		public List<Group> Groups = new List<Group>();
		private static string _groupRelationshipName = "new_group_contact";
		private string _accountRelationshipName = "new_account_contact";
		private string _indsamlerAccountRelationshipName = "new_account_contact_indsamlere";
		private string _annotationRelationshipName = "Contact_Annotation";

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
			"new_indsamler2016",

			"address1_addressid",
			"address2_addressid",
			"address3_addressid",

			"createdon",
			"modifiedon",
			"modifiedby",

			"firstname",
			"middlename",
			"lastname",
			"new_oprindelse",
			"new_oprindelseip",
			"new_mailrelaysubscriberid",
			"new_mailrelaycheck",
			"statecode");

		protected override string entityName { get { return "contact"; } }
		protected override string idName { get { return "contactid"; } }

		private static readonly ColumnSet ColumnSetContactCrmGenerated = new ColumnSet("address1_addressid", "createdon", "modifiedon", "modifiedby", "address3_addressid", "address2_addressid", "statecode");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetContactCrmGenerated; } }

		public Contact(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public Contact(DynamicsCrmConnection connection, Entity contactEntity) : base(connection, contactEntity)
		{
			if (contactEntity.Attributes.Any(attribute => attribute.Key == "statecode"))
			{
				State = (StateEnum)((OptionSetValue)contactEntity.Attributes["statecode"]).Value;
			}
		}

		public Guid? modifiedbyGuid
		{
			get
			{
				if (modifiedby == null)
				{
					return null;
				}

				return modifiedby.Id;
			}
		}

		private void ReadGroups(Entity contactEntity)
		{
			Groups = ReadNNRelationship(_groupRelationshipName, contactEntity, entity => new Group(entity));
		}

		protected override CrmEntity GetAsEntity(bool includeContactId)
		{
			CrmEntity crmEntity = new CrmEntity("contact");

			if (includeContactId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("contactid", Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("firstname", firstname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("middlename", middlename));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("lastname", lastname));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_hargavebrev", new_hargavebrev));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminstatus", new_kkadminstatus));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_gavebrevudloebsdato", new_gavebrevudloebsdato));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_storkredsnavn", new_storkredsnavn));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_storkredsnr", new_storkredsnr));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminsoegenavn", new_kkadminsoegenavn));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminmedlemsnr", new_kkadminmedlemsnr));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_titel", new_titel));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_indsamler2016", new_indsamler2016));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line1", address1_line1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line2", address1_line2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_city", address1_city));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_postalcode", address1_postalcode));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("emailaddress1", emailaddress1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("mobilephone", mobilephone));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("telephone1", telephone1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("birthdate", birthdate));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_oprindelse", new_oprindelse));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_oprindelseip", new_oprindelseip));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_mailrelaysubscriberid", new_mailrelaysubscriberid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_mailrelaycheck", new_mailrelaycheck));

			return crmEntity;
		}

		private static readonly DateTime _minimumSearchDate = new DateTime(1900, 1, 1);
		public static List<Contact> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfContacts = null)
		{
			List<Contact> contacts = StaticCrm.ReadLatest(connection, "contact", ColumnSetContact, lastSearchDate, (lConnection, entity) => new Contact(lConnection, entity), maximumNumberOfContacts);

			return contacts;
		}

		public List<Annotation> GetAnnotations()
		{
			Entity currentEntity = GetAsEntity(true);

			return ReadNNRelationship(_annotationRelationshipName, currentEntity, entity => new Annotation(Connection, entity));
		}

		public void SynchronizeAnnotations(List<Annotation> annotations)
		{
			Entity currentEntity = GetAsEntity(true);
			List<Guid> annotationIds = annotations.Select(annotation => annotation.Id).ToList();

			SynchronizeNNRelationship(currentEntity, _annotationRelationshipName, "annotation", "annotationid", annotationIds, SynchronizeActionEnum.Delete);
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

			Contact contact = new Contact(connection, contactEntity);
			contact.ReadGroups(contactEntity);

			return contact;
		}

		protected override void AfterInsert(Entity generatedEntity)
		{
			SynchronizeGroupsInCrm(generatedEntity);
		}

		protected override void AfterUpdate(Entity generatedEntity)
		{
			SynchronizeGroupsInCrm(generatedEntity);
		}

		private void SynchronizeGroupsInCrm(Entity contactEntity)
		{
			SynchronizeNNRelationship(contactEntity, _groupRelationshipName, "new_group", "new_groupid", Groups.Select(group => group.GroupId).ToList(), SynchronizeActionEnum.Disassociate);
		}

		public void SynchronizeGroups()
		{
			List<Guid> groupIds = Groups.Select(group => group.GroupId).ToList();

			SynchronizeGroups(groupIds);
		}

		public void SynchronizeGroups(List<string> groupNames)
		{
			List<Guid> groupIds = groupNames.Select(groupName => Group.ReadOrCreate(Connection, groupName).GroupId).ToList();

			SynchronizeGroups(groupIds);
		}

		public void SynchronizeGroups(List<Guid> groupIds)
		{
			Entity currentEntity = GetAsEntity(true);

			SynchronizeNNRelationship(currentEntity, _groupRelationshipName, "new_group", "new_groupid", groupIds, SynchronizeActionEnum.Disassociate);
		}

		public void SynchronizeAccounts(List<Account> accounts)
		{
			List<Guid> accountIds = accounts.Select(account => account.Id).ToList();
			SynchronizeAccounts(accountIds);
		}

		public void SynchronizeAccounts(List<Guid> accountIds)
		{
			Entity currentEntity = GetAsEntity(true);

			SynchronizeNNRelationship(currentEntity, _accountRelationshipName, "account", "accountid", accountIds, SynchronizeActionEnum.Disassociate);
		}

		public void SynchronizeIndsamlere(List<Account> indsamlerAccounts)
		{
			List<Guid> indsamlerAccountIds = indsamlerAccounts.Select(account => account.Id).ToList();
			SynchronizeIndsamlere(indsamlerAccountIds);
		}

		public void SynchronizeIndsamlere(List<Guid> indsamlerAccountIds)
		{
			Entity currentEntity = GetAsEntity(true);

			SynchronizeNNRelationship(currentEntity, _indsamlerAccountRelationshipName, "account", "accountid", indsamlerAccountIds, SynchronizeActionEnum.Disassociate);
		}

		public List<Guid> GetExternalAccountIdsFromAccountContact()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, _accountRelationshipName);

			List<Guid> externalIds = relatedEntities.Select(entity => entity.GetAttributeValue<Guid>("accountid")).ToList();

			return externalIds;
		}

		public List<Group> GetExternalGroupsFromContactGroup()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, _groupRelationshipName);

			List<Group> externalGroups = relatedEntities.Select(entity => new Group(entity)).ToList();

			return externalGroups;
		}

		public List<Guid> GetExternalAccountIdsFromAccountIndsamlere()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, _indsamlerAccountRelationshipName);

			List<Guid> externalIds = relatedEntities.Select(entity => entity.GetAttributeValue<Guid>("accountid")).ToList();

			return externalIds;
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

		public void SetActive(bool isActive)
		{
			SetStateRequest setStateRequest = new SetStateRequest()
			{
				EntityMoniker = new EntityReference
				{
					Id = Id,
					LogicalName = entityName,
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

			Connection.Service.Execute(setStateRequest);
		}

		public static bool Exists(DynamicsCrmConnection dynamicsCrmConnection, Dictionary<string, string> keyContent)
		{
			return StaticCrm.Exists<Contact>(dynamicsCrmConnection, keyContent);
		}

		public static Contact Create(DynamicsCrmConnection dynamicsCrmConnection, Dictionary<string, string> allContent)
		{
			Contact contact = new Contact(dynamicsCrmConnection);
			CreateFromContent(dynamicsCrmConnection, contact, allContent);
			return contact;
		}

		public static List<Contact> ReadFromFetchXml(DynamicsCrmConnection dynamicsCrmConnection, List<string> fields, Dictionary<string, string> keyContent)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, fields, keyContent, null, (connection, contactEntity) => new Contact(connection, contactEntity), new PagingInformation());
		}
	}
}
