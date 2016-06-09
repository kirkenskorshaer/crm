using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInterface.Dynamics.Crm
{
	public class Account : AbstractCrm
	{
		public string name;
		public StateEnum State { get; private set; }

		public string address1_line1;
		public string address1_line2;
		public string address1_city;
		public string address1_postalcode;
		public EntityReference modifiedby;
		public string emailaddress1;
		public string telephone1;

		public Guid address1_addressid { get; private set; }
		public Guid address2_addressid { get; private set; }

		public DateTime createdon { get; private set; }
		public DateTime modifiedon { get; private set; }

		public OptionSetValue new_erindsamlingssted;
		public int new_kkadminmedlemsnr;
		public int? new_forventetantalindsamlere2016;

		public EntityReference new_bykoordinatorid;
		public EntityReference new_omraadekoordinatorid;
		public EntityReference primarycontactid;
		public EntityReference new_korshaerslederid;
		public EntityReference new_genbrugskonsulentid;
		public EntityReference new_indsamlingskoordinatorid;
		public EntityReference new_byarbejdeid;
		public OptionSetValue new_kredsellerby;
		public OptionSetValue new_region;
		public OptionSetValue new_stedtype;

		private string _contactRelationshipName = "new_account_contact";
		private string _indsamlerRelationshipName = "new_account_contact_indsamlere";
		private string _groupRelationshipName = "new_account_new_group";
		private string _annotationRelationshipName = "Account_Annotation";

		public EntityReference new_leveringstedid;
		public Guid? leveringstedid { get { return GetEntityReferenceId(new_leveringstedid); } set { new_leveringstedid = SetEntityReferenceId(value, "account"); } }

		public EntityReference new_leveringkontaktid;
		public Guid? leveringkontaktid { get { return GetEntityReferenceId(new_leveringkontaktid); } set { new_leveringkontaktid = SetEntityReferenceId(value, "contact"); } }

		private static readonly DateTime _minimumSearchDate = new DateTime(1900, 1, 1);

		private static readonly ColumnSet ColumnSetAccount = new ColumnSet(
			"name",
			"accountid",

			"statecode",

			"address1_line1",
			"address1_line2",
			"address1_city",
			"address1_postalcode",
			"emailaddress1",
			"telephone1",

			"address1_addressid",
			"address2_addressid",

			"createdon",
			"modifiedon",
			"modifiedby",

			"new_erindsamlingssted",
			"new_kkadminmedlemsnr",
			"new_forventetantalindsamlere2016",
			"new_bykoordinatorid",
			"new_omraadekoordinatorid",
			"new_korshaerslederid",
			"new_genbrugskonsulentid",
			"new_indsamlingskoordinatorid",
			"new_byarbejdeid",
			"primarycontactid",
			"new_leveringstedid",
			"new_leveringkontaktid",
			"new_kredsellerby",
			"new_region",
			"new_stedtype"
		);

		private static readonly ColumnSet ColumnSetAccountCrmGenerated = new ColumnSet("address1_addressid", "createdon", "modifiedon", "modifiedby", "address2_addressid", "statecode");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetAccountCrmGenerated; } }

		protected override string entityName { get { return "account"; } }
		protected override string idName { get { return "accountid"; } }

		public Account(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public Account(DynamicsCrmConnection connection, Entity entity) : base(connection, entity)
		{
			State = (StateEnum)((OptionSetValue)entity.Attributes["statecode"]).Value;
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

		public Guid? bykoordinatorid
		{
			get
			{
				if (new_bykoordinatorid == null)
				{
					return null;
				}

				return new_bykoordinatorid.Id;
			}
			set
			{
				if (value == null)
				{
					new_bykoordinatorid = null;
				}

				new_bykoordinatorid = new EntityReference("contact", value.Value);
			}
		}

		public Guid? omraadekoordinatorid
		{
			get
			{
				if (new_omraadekoordinatorid == null)
				{
					return null;
				}

				return new_omraadekoordinatorid.Id;
			}
			set
			{
				if (value == null)
				{
					new_omraadekoordinatorid = null;
				}

				new_omraadekoordinatorid = new EntityReference("contact", value.Value);
			}
		}

		public Guid? korshaerslederid
		{
			get
			{
				if (new_korshaerslederid == null)
				{
					return null;
				}

				return new_korshaerslederid.Id;
			}
			set
			{
				if (value == null)
				{
					new_korshaerslederid = null;
				}

				new_korshaerslederid = new EntityReference("contact", value.Value);
			}
		}

		public Guid? genbrugskonsulentid
		{
			get
			{
				if (new_genbrugskonsulentid == null)
				{
					return null;
				}

				return new_genbrugskonsulentid.Id;
			}
			set
			{
				if (value == null)
				{
					new_genbrugskonsulentid = null;
				}

				new_genbrugskonsulentid = new EntityReference("contact", value.Value);
			}
		}

		public Guid? indsamlingskoordinatorid
		{
			get
			{
				if (new_indsamlingskoordinatorid == null)
				{
					return null;
				}

				return new_indsamlingskoordinatorid.Id;
			}
			set
			{
				if (value == null)
				{
					new_indsamlingskoordinatorid = null;
				}

				new_indsamlingskoordinatorid = new EntityReference("contact", value.Value);
			}
		}

		public Guid? byarbejdeid
		{
			get
			{
				if (new_byarbejdeid == null)
				{
					return null;
				}

				return new_byarbejdeid.Id;
			}
			set
			{
				if (value == null)
				{
					new_byarbejdeid = null;
				}

				new_byarbejdeid = new EntityReference("new_byarbejde", value.Value);
			}
		}

		public Guid? primarycontact
		{
			get
			{
				if (primarycontactid == null)
				{
					return null;
				}

				return primarycontactid.Id;
			}
			set
			{
				if (value == null)
				{
					primarycontactid = null;
				}

				primarycontactid = new EntityReference("contact", value.Value);
			}
		}

		public enum kredsellerbyEnum
		{
			kreds = 100000000,
			by = 100000001,
		}

		public enum regionEnum
		{
			nord = 100000000,
			syd = 100000001,
			øst = 100000002,
			midt = 100000003,
		}

		public enum stedtypeEnum
		{
			Kredsarbejde = 100000000,
			Byarbejde = 100000001,
			Arbejdssted = 100000002,
			Herberg = 100000003,
			Sognegård = 100000004,
			Andet = 100000005,
			Butik = 100000006,
		}

		public enum erindsamlingsstedEnum
		{
			Ja = 100000000,
			Nej = 100000001,
			Afventer = 100000002,
		}

		public kredsellerbyEnum? kredsellerby
		{
			get
			{
				if (new_kredsellerby == null)
				{
					return null;
				}

				return (kredsellerbyEnum)new_kredsellerby.Value;
			}
			set
			{
				if (value == null)
				{
					new_kredsellerby = null;
				}

				new_kredsellerby = new OptionSetValue((int)value.Value);
			}
		}

		public regionEnum? region
		{
			get
			{
				if (new_region == null)
				{
					return null;
				}

				return (regionEnum)new_region.Value;
			}
			set
			{
				if (value == null)
				{
					new_region = null;
				}

				new_region = new OptionSetValue((int)value.Value);
			}
		}

		public erindsamlingsstedEnum? erindsamlingssted
		{
			get
			{
				if (new_erindsamlingssted == null)
				{
					return null;
				}

				return (erindsamlingsstedEnum)new_erindsamlingssted.Value;
			}
			set
			{
				if (value == null)
				{
					new_erindsamlingssted = null;
				}

				new_erindsamlingssted = new OptionSetValue((int)value.Value);
			}
		}

		public stedtypeEnum? stedtype
		{
			get
			{
				if (new_stedtype == null)
				{
					return null;
				}

				return (stedtypeEnum)new_stedtype.Value;
			}
			set
			{
				if (value == null)
				{
					new_stedtype = null;
				}

				new_stedtype = new OptionSetValue((int)value.Value);
			}
		}

		public static List<Account> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfAccounts = null)
		{
			List<Account> accounts = StaticCrm.ReadLatest(connection, "account", ColumnSetAccount, lastSearchDate, (lConnection, entity) => new Account(lConnection, entity), maximumNumberOfAccounts);

			return accounts;
		}

		public List<Annotation> GetAnnotations()
		{
			Entity currentEntity = GetAsEntity(true);

			return ReadNNRelationship(_annotationRelationshipName, currentEntity, entity => new Annotation(Connection, entity));
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity("account");

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("accountid", Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("name", name));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line1", address1_line1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_line2", address1_line2));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_city", address1_city));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("address1_postalcode", address1_postalcode));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("emailaddress1", emailaddress1));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("telephone1", telephone1));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_erindsamlingssted", new_erindsamlingssted));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kkadminmedlemsnr", new_kkadminmedlemsnr));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_forventetantalindsamlere2016", new_forventetantalindsamlere2016));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_bykoordinatorid", new_bykoordinatorid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_omraadekoordinatorid", new_omraadekoordinatorid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_korshaerslederid", new_korshaerslederid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_genbrugskonsulentid", new_genbrugskonsulentid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_indsamlingskoordinatorid", new_indsamlingskoordinatorid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_byarbejdeid", new_byarbejdeid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_leveringstedid", new_leveringstedid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_leveringkontaktid", new_leveringkontaktid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("primarycontactid", primarycontactid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kredsellerby", new_kredsellerby));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_region", new_region));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_stedtype", new_stedtype));

			return crmEntity;
		}

		public static Account Read(DynamicsCrmConnection connection, Guid accountid)
		{
			Entity accountEntity = connection.Service.Retrieve("account", accountid, ColumnSetAccount);

			Account account = new Account(connection, accountEntity);

			return account;
		}

		public List<Group> ReadGroups()
		{
			Entity accountEntity = GetAsEntity(true);

			List<Group> groups = ReadNNRelationship(_groupRelationshipName, accountEntity, entity => new Group(entity));

			return groups;
		}

		public List<Contact> ReadContacts()
		{
			Entity accountEntity = GetAsEntity(true);

			List<Contact> contacts = ReadNNRelationship(_contactRelationshipName, accountEntity, entity => new Contact(Connection, entity));

			return contacts;
		}

		public List<Contact> ReadIndsamlere()
		{
			Entity accountEntity = GetAsEntity(true);

			List<Contact> indsamlere = ReadNNRelationship(_indsamlerRelationshipName, accountEntity, entity => new Contact(Connection, entity));

			return indsamlere;
		}

		public void SynchronizeContacts(List<Contact> contacts)
		{
			List<Guid> contactIds = contacts.Select(contact => contact.Id).ToList();

			SynchronizeContacts(contactIds);
		}

		public void SynchronizeContacts(List<Guid> contactIds)
		{
			Entity currentEntity = GetAsEntity(true);

			SynchronizeNNRelationship(currentEntity, _contactRelationshipName, "contact", "contactid", contactIds, SynchronizeActionEnum.Disassociate);
		}

		public void SynchronizeIndsamlere(List<Contact> indsamlere)
		{
			List<Guid> indsamlerIds = indsamlere.Select(contact => contact.Id).ToList();

			SynchronizeIndsamlere(indsamlerIds);
		}

		public void SynchronizeIndsamlere(List<Guid> indsamlerIds)
		{
			Entity currentEntity = GetAsEntity(true);

			SynchronizeNNRelationship(currentEntity, _indsamlerRelationshipName, "contact", "contactid", indsamlerIds, SynchronizeActionEnum.Disassociate);
		}


		public void SynchronizeGroups(List<Group> groups)
		{
			List<Guid> groupIds = groups.Select(group => group.GroupId).ToList();

			SynchronizeGroups(groupIds);
		}

		public void SynchronizeGroups(List<Guid> groupIds)
		{
			Entity currentEntity = GetAsEntity(true);

			SynchronizeNNRelationship(currentEntity, _groupRelationshipName, "new_group", "new_groupid", groupIds, SynchronizeActionEnum.Disassociate);
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

		public List<Guid> GetExternalContactIdsFromAccountContact()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, _contactRelationshipName);

			List<Guid> externalIds = relatedEntities.Select(entity => entity.GetAttributeValue<Guid>("contactid")).ToList();

			return externalIds;
		}

		public List<Guid> GetExternalContactIdsFromAccountIndsamler()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, _indsamlerRelationshipName);

			List<Guid> externalIds = relatedEntities.Select(entity => entity.GetAttributeValue<Guid>("contactid")).ToList();

			return externalIds;
		}

		public List<Group> GetExternalGroupsFromAccountGroup()
		{
			Entity currentEntity = GetAsEntity(true);

			IEnumerable<Entity> relatedEntities = GetRelatedEntities(currentEntity, _groupRelationshipName);

			List<Group> externalGroups = relatedEntities.Select(entity => new Group(entity)).ToList();

			return externalGroups;
		}

		public void SynchronizeAnnotations(List<Annotation> annotations)
		{
			Entity currentEntity = GetAsEntity(true);
			List<Guid> annotationIds = annotations.Select(annotation => annotation.Id).ToList();

			SynchronizeNNRelationship(currentEntity, _annotationRelationshipName, "annotation", "annotationid", annotationIds, SynchronizeActionEnum.Delete);
		}
	}
}
