using Microsoft.Xrm.Client;
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

		protected override ColumnSet ColumnSetCrmGenerated { get { throw new NotImplementedException(); } }

		public MarketingList(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public MarketingList(DynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		public static MarketingList GetListForMailrelayUpdate(DynamicsCrmConnection dynamicsCrmConnection, PagingInformation pagingInformation, Guid? crmListId)
		{
			List<string> fields = new List<string>()
			{
				"new_mailrelaygroupid",
				"listname",
				"query",
				"new_mailrelaycheck",
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

		private List<KeyValueEntity<Guid, int?>> GetCrmIdsAndSubscriberIds()
		{
			XDocument xDocument = XDocument.Parse(query);

			string targetEntityName = "contact";
			string targetIdName = "contactid";

			string keyName = "contactid";
			string valueName = "new_mailrelaysubscriberid";

			xDocument.Descendants().Where(descendant => descendant.Name == "attribute").Remove();

			xDocument.Element("fetch").Element("entity").Add(new XElement("attribute", new XAttribute("name", keyName), new XAttribute("alias", "key")));
			xDocument.Element("fetch").Element("entity").Add(new XElement("attribute", new XAttribute("name", valueName), new XAttribute("alias", "value")));

			XElement filterElement = xDocument.Element("fetch").Element("entity").Element("filter");

			if (filterElement == null)
			{
				xDocument.Element("fetch").Element("entity").Add(new XElement("filter"));
			}

			xDocument.Element("fetch").Element("entity").Element("filter").Add(new XElement("condition", new XAttribute("attribute", "new_mailrelaysubscriberid"), new XAttribute("operator", "not-null")));

			List<KeyValueEntity<Guid, int?>> keyValueEntities = StaticCrm.ReadFromFetchXml(Connection, xDocument, (dynamicsCrmConnection, entity) => new KeyValueEntity<Guid, int?>(dynamicsCrmConnection, entity, targetEntityName, targetIdName), new PagingInformation());

			return keyValueEntities;
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

		protected override CrmEntity GetAsEntity(bool includeMarketingListId)
		{
			CrmEntity crmEntity = new CrmEntity("list");

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

		public void UpdateMailrelaycheck(DynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, entityName, idName, Id, new Dictionary<string, object>()
			{
				{ "new_mailrelaycheck", new_mailrelaycheck }
			});
		}

		public void UpdateMailrelaygroupid(DynamicsCrmConnection dynamicsCrmConnection)
		{
			Update(dynamicsCrmConnection, entityName, idName, Id, new Dictionary<string, object>()
			{
				{ "new_mailrelaygroupid", new_mailrelaygroupid }
			});
		}

		public static MarketingList Read(DynamicsCrmConnection connection, Guid marketinglistId)
		{
			Entity marketingListEntity = connection.Service.Retrieve("list", marketinglistId, ColumnSetMarketinglist);

			MarketingList marketingList = new MarketingList(connection, marketingListEntity);

			return marketingList;
		}
	}
}
