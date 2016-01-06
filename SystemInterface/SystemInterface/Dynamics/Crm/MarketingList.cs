using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class MarketingList
	{
		public Guid listid;
		public string query;
		public createdfromcodeEnum createdfromcode = createdfromcodeEnum.Contact;
		public string listname;

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
			"createdfromcode"
		);

		private CrmEntity GetMarketingListAsEntity(bool includeMarketingListId)
		{
			CrmEntity crmEntity = new CrmEntity("list");

			if (includeMarketingListId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("listid", listid));
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("query", query));

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("type", true));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("listname", listname));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("createdfromcode", new OptionSetValue((int)createdfromcode)));

			return crmEntity;
		}

		private static MarketingList EntityToMarketingList(Entity entity)
		{
			MarketingList marketingList = new MarketingList();

			marketingList.createdfromcode = (createdfromcodeEnum)((OptionSetValue)entity.Attributes["createdfromcode"]).Value;

			IEnumerable<string> keys = entity.Attributes.Keys.Where(key => key != "createdfromcode");

			foreach (string key in keys)
			{
				Utilities.ReflectionHelper.SetValue(marketingList, key, entity.Attributes[key]);
			}

			return marketingList;
		}

		public void Insert(DynamicsCrmConnection connection)
		{
			CrmEntity crmEntity = GetMarketingListAsEntity(false);

			listid = connection.Service.Create(crmEntity);
		}

		public static MarketingList Read(DynamicsCrmConnection connection, Guid marketinglistId)
		{
			Entity marketingListEntity = connection.Service.Retrieve("list", marketinglistId, ColumnSetMarketinglist);

			MarketingList marketingList = EntityToMarketingList(marketingListEntity);

			return marketingList;
		}

		public void Delete(DynamicsCrmConnection connection)
		{
			connection.Service.Delete("list", listid);
		}
	}
}
