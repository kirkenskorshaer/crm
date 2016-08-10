using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public static class StaticCrm
	{
		private static readonly DateTime _minimumSearchDate = new DateTime(1900, 1, 1);
		public static List<AbstractCrmType> ReadLatest<AbstractCrmType>
		(
			DynamicsCrmConnection connection
			, string entityName
			, ColumnSet ColumnSetCrmType
			, DateTime lastSearchDate
			, Func<DynamicsCrmConnection, Entity, AbstractCrmType> CrmTypeConstructor
			, int? maximumNumberOfEntities = null
		)
		where AbstractCrmType : AbstractCrm
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

			QueryExpression query = new QueryExpression(entityName)
			{
				ColumnSet = ColumnSetCrmType
			};
			query.Criteria.AddFilter(filterExpression);

			if (maximumNumberOfEntities.HasValue)
			{
				query.TopCount = maximumNumberOfEntities.Value;
				query.AddOrder("modifiedon", OrderType.Descending);
			}

			EntityCollection entityCollection = connection.Service.RetrieveMultiple(query);

			List<AbstractCrmType> crmEntities = entityCollection.Entities.Select(entity => CrmTypeConstructor(connection, entity)).ToList();

			return crmEntities;
		}

		public static List<AbstractCrmType> ReadByAttribute<AbstractCrmType>
		(
			DynamicsCrmConnection connection
			, string entityName
			, ColumnSet ColumnSetCrmType
			, string attributeName
			, object attributeValue
			, Func<DynamicsCrmConnection, Entity, AbstractCrmType> CrmTypeConstructor
			, int? maximumNumberOfEntities = null
		)
		where AbstractCrmType : AbstractCrm
		{
			ConditionExpression equalsExpression = new ConditionExpression
			{
				AttributeName = attributeName,
				Operator = ConditionOperator.Equal
			};
			equalsExpression.Values.Add(attributeValue);

			FilterExpression filterExpression = new FilterExpression();
			filterExpression.Conditions.Add(equalsExpression);

			QueryExpression query = new QueryExpression(entityName)
			{
				ColumnSet = ColumnSetCrmType
			};
			query.Criteria.AddFilter(filterExpression);

			if (maximumNumberOfEntities.HasValue)
			{
				query.TopCount = maximumNumberOfEntities.Value;
			}

			EntityCollection entityCollection = connection.Service.RetrieveMultiple(query);

			List<AbstractCrmType> crmEntities = entityCollection.Entities.Select(entity => CrmTypeConstructor(connection, entity)).ToList();

			return crmEntities;
		}

		public static List<AbstractCrmType> ReadFromFetchXml<AbstractCrmType>(DynamicsCrmConnection dynamicsCrmConnection, List<string> fields, Dictionary<string, string> keyContent, int? maxCount, Func<DynamicsCrmConnection, Entity, AbstractCrmType> CrmTypeConstructor, PagingInformation pagingInformation)
		where AbstractCrmType : AbstractCrm
		{
			return ReadFromFetchXml(dynamicsCrmConnection, typeof(AbstractCrmType).Name.ToLower(), fields, keyContent, maxCount, CrmTypeConstructor, pagingInformation);
		}

		public static List<AbstractCrmType> ReadFromFetchXml<AbstractCrmType>(DynamicsCrmConnection dynamicsCrmConnection, string entityName, List<string> fields, Dictionary<string, string> keyContent, int? maxCount, Func<DynamicsCrmConnection, Entity, AbstractCrmType> CrmTypeConstructor, PagingInformation pagingInformation)
		where AbstractCrmType : AbstractCrm
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", entityName),
						new XElement("filter"))));

			if (maxCount.HasValue)
			{
				xDocument.Element("fetch").Add(new XAttribute("count", maxCount.Value));
			}

			xDocument.Element("fetch").Element("entity").Add(GetAttributeElements(fields.ToList()));
			xDocument.Element("fetch").Element("entity").Element("filter").Add(GetConditionElements(keyContent));

			List<AbstractCrmType> crmObjects = ReadFromFetchXml(dynamicsCrmConnection, xDocument, CrmTypeConstructor, pagingInformation);

			return crmObjects;
		}

		public static List<AbstractCrmType> ReadFromFetchXml<AbstractCrmType>(DynamicsCrmConnection dynamicsCrmConnection, string path, Func<DynamicsCrmConnection, Entity, AbstractCrmType> CrmTypeConstructor, PagingInformation pagingInformation)
		where AbstractCrmType : AbstractCrm
		{
			XDocument xDocument = XDocument.Load(path);

			return ReadFromFetchXml(dynamicsCrmConnection, xDocument, CrmTypeConstructor, pagingInformation);
		}

		public static List<AbstractCrmType> ReadFromFetchXml<AbstractCrmType>(DynamicsCrmConnection dynamicsCrmConnection, XDocument xDocument, Func<DynamicsCrmConnection, Entity, AbstractCrmType> CrmTypeConstructor, PagingInformation pagingInformation)
		where AbstractCrmType : AbstractCrm
		{
			if (pagingInformation.FirstRun == false)
			{
				if (pagingInformation.MoreRecords == false)
				{
					return new List<AbstractCrmType>();
				}

				xDocument.Element("fetch").Add(new XAttribute("paging-cookie", pagingInformation.PagingCookie));
				xDocument.Element("fetch").Add(new XAttribute("page", pagingInformation.Page));
			}

			pagingInformation.FirstRun = false;

			FetchExpression fetchExpression = new FetchExpression(xDocument.ToString());

			EntityCollection entityCollection = dynamicsCrmConnection.Service.RetrieveMultiple(fetchExpression);

			if (entityCollection.Entities.Count == 0)
			{
				return new List<AbstractCrmType>();
			}

			List<AbstractCrmType> crmEntities = entityCollection.Entities.Select(entity => CrmTypeConstructor(dynamicsCrmConnection, entity)).ToList();

			pagingInformation.MoreRecords = entityCollection.MoreRecords;
			pagingInformation.PagingCookie = entityCollection.PagingCookie;
			pagingInformation.Page++;

			return crmEntities;
		}

		public static List<string> GetAllAttributeNames(DynamicsCrmConnection connection, Type entityType)
		{
			RetrieveEntityRequest retrieveBankAccountEntityRequest = new RetrieveEntityRequest
			{
				EntityFilters = EntityFilters.Attributes,
				LogicalName = entityType.Name.ToLower()
			};
			RetrieveEntityResponse response = (RetrieveEntityResponse)connection.Service.Execute(retrieveBankAccountEntityRequest);

			List<string> attributeNames = response.EntityMetadata.Attributes.Select(attribute => attribute.SchemaName).ToList();

			return attributeNames;
		}

		public static int CountByFetchXml(DynamicsCrmConnection dynamicsCrmConnection, string path, string aliasName)
		{
			XDocument xDocument = XDocument.Load(path);

			return CountByFetchXml(dynamicsCrmConnection, xDocument, aliasName);
		}

		public static int CountByFetchXml(DynamicsCrmConnection dynamicsCrmConnection, XDocument xDocument, string aliasName)
		{
			FetchExpression fetchExpression = new FetchExpression(xDocument.ToString());

			EntityCollection entityCollection = dynamicsCrmConnection.Service.RetrieveMultiple(fetchExpression);

			int collectionCount = entityCollection.Entities.Count;

			if (collectionCount == 0)
			{
				return 0;
			}

			if (collectionCount != 1)
			{
				throw new Exception("wrong collection count");
			}

			Entity entity = entityCollection.Entities[0];

			AliasedValue aliasedValue = (AliasedValue)entity.Attributes[aliasName];

			return (int)aliasedValue.Value;
		}

		public static bool ExistsByFetchXml(DynamicsCrmConnection dynamicsCrmConnection, string path)
		{
			XDocument xDocument = XDocument.Load(path);

			return ExistsByFetchXml(dynamicsCrmConnection, xDocument);
		}

		public static bool ExistsByFetchXml(DynamicsCrmConnection dynamicsCrmConnection, XDocument xDocument)
		{
			FetchExpression fetchExpression = new FetchExpression(xDocument.ToString());

			EntityCollection entityCollection = dynamicsCrmConnection.Service.RetrieveMultiple(fetchExpression);

			int collectionCount = entityCollection.Entities.Count;

			return collectionCount >= 1;
		}

		public static bool Exists<AbstractCrmType>(DynamicsCrmConnection dynamicsCrmConnection, Dictionary<string, string> keyContent)
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch", new XAttribute("count", 1),
					new XElement("entity", new XAttribute("name", typeof(AbstractCrmType).Name.ToLower()),
						new XElement("filter"))));

			xDocument.Element("fetch").Element("entity").Element("filter").Add(GetConditionElements(keyContent));

			bool exists = ExistsByFetchXml(dynamicsCrmConnection, xDocument);

			return exists;
		}

		public static List<XElement> GetAttributeElements(List<string> attributes)
		{
			List<XElement> xElements = new List<XElement>();

			foreach (string name in attributes)
			{
				XElement condition = new XElement("attribute", new XAttribute("name", name));
				xElements.Add(condition);
			}

			return xElements;
		}

		public static List<XElement> GetConditionElements(Dictionary<string, string> searchContent)
		{
			List<XElement> xElements = new List<XElement>();

			foreach (KeyValuePair<string, string> keyValue in searchContent)
			{
				string key = keyValue.Key;
				string value = keyValue.Value;

				XElement condition = new XElement("condition", new XAttribute("attribute", key), new XAttribute("operator", "eq"), new XAttribute("value", value));

				xElements.Add(condition);
			}

			return xElements;
		}
	}
}
