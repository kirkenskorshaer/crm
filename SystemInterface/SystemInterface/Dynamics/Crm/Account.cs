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
		public string emailaddress1;
		public string telephone1;

		public Guid address1_addressid { get; private set; }
		public Guid address2_addressid { get; private set; }

		public DateTime createdon { get; private set; }
		public DateTime modifiedon { get; private set; }

		public bool new_erindsamlingssted;

		private string _contactRelationshipName = "new_account_contact";
		private string _indsamlerRelationshipName = "new_account_contact_indsamlere";
		private string _groupRelationshipName = "new_account_new_group";

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

			"new_erindsamlingssted"
		);

		private static readonly ColumnSet ColumnSetAccountCrmGenerated = new ColumnSet("address1_addressid", "createdon", "modifiedon", "address2_addressid", "statecode");
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

		public static List<Account> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfAccounts = null)
		{
			List<Account> accounts = StaticCrm.ReadLatest(connection, "account", ColumnSetAccount, lastSearchDate, (lConnection, entity) => new Account(lConnection, entity), maximumNumberOfAccounts);

			return accounts;
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

			return crmEntity;
		}

		public static Account Read(DynamicsCrmConnection connection, Guid accountid)
		{
			Entity accountEntity = connection.Service.Retrieve("account", accountid, ColumnSetAccount);

			Account account = new Account(connection, accountEntity);

			return account;
		}

		public List<Group> ReadGroups(Entity accountEntity)
		{
			List<Group> groups = ReadNNRelationship(_groupRelationshipName, accountEntity, entity => new Group(entity));

			return groups;
		}

		public List<Contact> ReadContacts(Entity accountEntity)
		{
			List<Contact> contacts = ReadNNRelationship(_contactRelationshipName, accountEntity, entity => new Contact(Connection, entity));

			return contacts;
		}

		public List<Contact> ReadIndsamlere(Entity accountEntity)
		{
			List<Contact> indsamlere = ReadNNRelationship(_indsamlerRelationshipName, accountEntity, entity => new Contact(Connection, entity));

			return indsamlere;
		}

		public void SynchronizeContacts(List<Contact> contacts)
		{
			Entity currentEntity = GetAsEntity(true);

			List<Guid> contactIds = contacts.Select(contact => contact.Id).ToList();

			SynchronizeNNRelationship(currentEntity, _contactRelationshipName, "contact", "contactid", contactIds);
		}

		public void SynchronizeIndsamlere(List<Contact> indsamlere)
		{
			Entity currentEntity = GetAsEntity(true);

			List<Guid> indsamlerIds = indsamlere.Select(contact => contact.Id).ToList();

			SynchronizeNNRelationship(currentEntity, _indsamlerRelationshipName, "contact", "contactid", indsamlerIds);
		}

		public void SynchronizeGroups(List<Group> groups)
		{
			Entity currentEntity = GetAsEntity(true);

			List<Guid> groupIds = groups.Select(group => group.GroupId).ToList();

			SynchronizeNNRelationship(currentEntity, _groupRelationshipName, "new_group", "new_groupid", groupIds);
		}
	}
}
