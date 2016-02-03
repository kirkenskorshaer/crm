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

		protected override string entityName { get { return "contact"; } }
		protected override string idName { get { return "contactid"; } }

		private static readonly ColumnSet ColumnSetContactCrmGenerated = new ColumnSet("address1_addressid", "createdon", "modifiedon", "address3_addressid", "address2_addressid", "statecode");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetContactCrmGenerated; } }

		public Contact(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public Contact(DynamicsCrmConnection connection, Entity contactEntity) : base(connection, contactEntity)
		{
			State = (StateEnum)((OptionSetValue)contactEntity.Attributes["statecode"]).Value;
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
		public static List<Contact> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfContacts = null)
		{
			List<Contact> contacts = StaticCrm.ReadLatest(connection, "contact", ColumnSetContact, lastSearchDate, (lConnection, entity) => new Contact(lConnection, entity), maximumNumberOfContacts);

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
			SynchronizeNNRelationship(contactEntity, _groupRelationshipName, "new_group", "new_groupid", Groups.Select(group => group.GroupId).ToList());
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
	}
}
