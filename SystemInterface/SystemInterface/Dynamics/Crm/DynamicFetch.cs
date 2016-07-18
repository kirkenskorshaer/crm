using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class DynamicFetch
	{
		public static List<dynamic> ReadFromFetchXml(DynamicsCrmConnection dynamicsCrmConnection, string entityName, List<string> fields, Dictionary<string, string> keyContent, int? maxCount, PagingInformation pagingInformation)
		{
			XDocument xDocument = new XDocument(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", entityName),
						new XElement("filter"))));

			if (maxCount.HasValue)
			{
				xDocument.Element("fetch").Add(new XAttribute("count", maxCount.Value));
			}

			xDocument.Element("fetch").Element("entity").Add(StaticCrm.GetAttributeElements(fields.ToList()));
			xDocument.Element("fetch").Element("entity").Element("filter").Add(StaticCrm.GetConditionElements(keyContent));

			List<dynamic> crmObjects = ReadFromFetchXml(dynamicsCrmConnection, xDocument, pagingInformation);

			return crmObjects;
		}

		public static List<dynamic> ReadFromFetchXml(DynamicsCrmConnection dynamicsCrmConnection, string path, PagingInformation pagingInformation)
		{
			XDocument xDocument = XDocument.Load(path);

			return ReadFromFetchXml(dynamicsCrmConnection, xDocument, pagingInformation);
		}

		public static List<dynamic> ReadFromFetchXml(DynamicsCrmConnection dynamicsCrmConnection, XDocument xDocument, PagingInformation pagingInformation)
		{
			if (pagingInformation.FirstRun == false)
			{
				if (pagingInformation.MoreRecords == false)
				{
					return new List<dynamic>();
				}

				xDocument.Element("fetch").Add(new XAttribute("paging-cookie", pagingInformation.PagingCookie));
				xDocument.Element("fetch").Add(new XAttribute("page", pagingInformation.Page));
			}

			pagingInformation.FirstRun = false;

			FetchExpression fetchExpression = new FetchExpression(xDocument.ToString());

			EntityCollection entityCollection = dynamicsCrmConnection.Service.RetrieveMultiple(fetchExpression);

			if (entityCollection.Entities.Count == 0)
			{
				return new List<dynamic>();
			}

			List<dynamic> crmEntities = entityCollection.Entities.Select(entity => GetFromEntity(entity)).ToList();

			pagingInformation.MoreRecords = entityCollection.MoreRecords;
			pagingInformation.PagingCookie = entityCollection.PagingCookie;
			pagingInformation.Page++;

			return crmEntities;
		}

		private static dynamic GetFromEntity(Entity entity)
		{
			dynamic dynamicFromEntity = new ExpandoObject();
			IDictionary<string, object> dynamicAsDictionary = (IDictionary<string, object>)dynamicFromEntity;

			foreach (KeyValuePair<string, object> attribute in entity.Attributes)
			{
				dynamicAsDictionary[attribute.Key] = attribute.Value;
			}

			return dynamicFromEntity;
		}
	}
}
