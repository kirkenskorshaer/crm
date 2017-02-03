using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utilities.Converter;

namespace SystemInterface.Dynamics.Crm
{
	public class MarketingList : AbstractCrm
	{
		public string query;
		public OptionSetValue createdfromcode;
		public createdfromcodeEnum? createdfrom { get { return GetOptionSet<createdfromcodeEnum>(createdfromcode); } set { createdfromcode = SetOptionSet((int?)value); } }
		public string listname;
		public string new_mailrelaycheck;
		public int? new_mailrelaygroupid;
		public bool? new_controlmailrelaygroup;

		protected override string entityName { get { return "list"; } }

		protected override string idName { get { return "listid"; } }

		private string targetEntityName
		{
			get
			{
				switch (createdfrom)
				{
					case createdfromcodeEnum.Account:
						return "account";
					case createdfromcodeEnum.Contact:
						return "contact";
					default:
						throw new ArgumentException($"unknown code {createdfrom}");
				}
			}
		}

		private string targetIdName
		{
			get
			{
				switch (createdfrom)
				{
					case createdfromcodeEnum.Account:
						return "accountid";
					case createdfromcodeEnum.Contact:
						return "contactid";
					default:
						throw new ArgumentException($"unknown code {createdfrom}");
				}
			}
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { throw new NotImplementedException(); } }

		public MarketingList(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public MarketingList(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		public static MarketingList GetListForMailrelayUpdate(IDynamicsCrmConnection dynamicsCrmConnection, PagingInformation pagingInformation, Guid? crmListId)
		{
			List<string> fields = new List<string>()
			{
				"new_mailrelaygroupid",
				"listname",
				"query",
				"new_mailrelaycheck",
				"createdfromcode",
			};

			Dictionary<string, string> search = new Dictionary<string, string>()
			{
				{ "new_controlmailrelaygroup", true.ToString() }
			};

			if (crmListId.HasValue)
			{
				search.Add("listid", crmListId.Value.ToString());
			}

			IEnumerable<MarketingList> lists = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, "list", fields, search, 1, (connection, entity) => new MarketingList(connection, entity), pagingInformation);

			return lists.SingleOrDefault();
		}

		public enum createdfromcodeEnum
		{
			Account = 1,
			Contact = 2,
		}

		private static readonly ColumnSet ColumnSetMarketinglist = new ColumnSet(
			"listid",
			"query",
			"type",
			"listname",
			"createdfromcode",
			"new_mailrelaycheck",
			"new_mailrelaygroupid",
			"new_controlmailrelaygroup"
		);

		private List<KeyValueEntity<Guid, int?>> _crmIdsAndSubscriberIds = null;
		public List<KeyValueEntity<Guid, int?>> CrmIdsAndSubscriberIds
		{
			get
			{
				if (_crmIdsAndSubscriberIds == null)
				{
					_crmIdsAndSubscriberIds = GetCrmIdsAndSubscriberIds();
				}

				return _crmIdsAndSubscriberIds;
			}
		}

		private IEnumerable<Guid> _contentIdsForNonMailrelaySubscribers = null;
		public IEnumerable<Guid> ContentIdsForNonMailrelaySubscribers
		{
			get
			{
				if (_contentIdsForNonMailrelaySubscribers == null)
				{
					_contentIdsForNonMailrelaySubscribers = GetContentIdsForNonMailrelaySubscribers();
				}

				return _contentIdsForNonMailrelaySubscribers;
			}
		}

		private List<KeyValueEntity<Guid, int?>> GetCrmIdsAndSubscriberIds()
		{
			XDocument xDocument = XDocument.Parse(query);

			XmlHelper.RemoveAllAttributes(xDocument);

			XmlHelper.AddAliasedValue(xDocument, "contactid", "key");
			XmlHelper.AddAliasedValue(xDocument, "new_mailrelaysubscriberid", "value");

			XmlHelper.AddCondition(xDocument, "new_mailrelaysubscriberid", "not-null");

			List<KeyValueEntity<Guid, int?>> keyValueEntities = StaticCrm.ReadFromFetchXml(Connection, xDocument, (dynamicsCrmConnection, entity) => new KeyValueEntity<Guid, int?>(dynamicsCrmConnection, entity, targetEntityName, targetIdName), new PagingInformation());

			return keyValueEntities;
		}

		private IEnumerable<Guid> GetContentIdsForNonMailrelaySubscribers()
		{
			XDocument xDocument = XDocument.Parse(query);

			XmlHelper.RemoveAllAttributes(xDocument);

			XmlHelper.AddAliasedValue(xDocument, "contactid", "value");

			XmlHelper.AddCondition(xDocument, "new_mailrelaysubscriberid", "null");

			XmlHelper.AddCondition(xDocument, "emailaddress1", "not-null");

			XmlHelper.SetCount(xDocument, 100);

			IEnumerable<Guid> contentIds = StaticCrm.ReadFromFetchXml(Connection, xDocument, (dynamicsCrmConnection, entity) => new SingleValueEntity<Guid>(dynamicsCrmConnection, entity, targetEntityName, targetIdName)).Select(valueEntity => valueEntity.value);

			return contentIds;
		}

		public bool RecalculateMarketingListCheck()
		{
			Guid newCheck = Guid.Empty;

			foreach (KeyValueEntity<Guid, int?> crmIdAndSubscriberId in CrmIdsAndSubscriberIds)
			{
				newCheck = Xor.XorGuid(newCheck, crmIdAndSubscriberId.key);
			}

			string newCheckString = newCheck.ToString();

			if (new_mailrelaycheck != null && new_mailrelaycheck.ToLower() == newCheckString.ToLower())
			{
				return false;
			}

			new_mailrelaycheck = newCheckString;
			return true;
		}

		protected override Entity GetAsEntity(bool includeMarketingListId)
		{
			Entity crmEntity = new Entity("list");

			if (includeMarketingListId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("listid", Id));
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("query", query));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("type", true));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("listname", listname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("createdfromcode", createdfromcode));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_mailrelaycheck", new_mailrelaycheck));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_mailrelaygroupid", new_mailrelaygroupid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_controlmailrelaygroup", new_controlmailrelaygroup));

			return crmEntity;
		}

		public void UpdateMailrelaycheck(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, entityName, idName, Id, new Dictionary<string, object>()
			{
				{ "new_mailrelaycheck", new_mailrelaycheck }
			});
		}

		public void UpdateMailrelaygroupid(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, entityName, idName, Id, new Dictionary<string, object>()
			{
				{ "new_mailrelaygroupid", new_mailrelaygroupid }
			});
		}

		public static MarketingList Read(IDynamicsCrmConnection connection, Guid marketinglistId)
		{
			Entity marketingListEntity = connection.Service.Retrieve("list", marketinglistId, ColumnSetMarketinglist);

			MarketingList marketingList = new MarketingList(connection, marketingListEntity);

			return marketingList;
		}
	}
}
