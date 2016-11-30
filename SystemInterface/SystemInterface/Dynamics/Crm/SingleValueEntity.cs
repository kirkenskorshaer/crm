using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class SingleValueEntity<ValueType> : AbstractValueEntity
	{
		public ValueType value;

		public SingleValueEntity(IDynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection, crmEntity, entityName, idName)
		{
		}

		public static List<ValueType> GetAll(IDynamicsCrmConnection dynamicsCrmConnection, XDocument xDocument, string entityName, string entityIdName)
		{
			IEnumerable<ValueType> entities = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (lDynamicsCrmConnection, entity) => new SingleValueEntity<ValueType>(lDynamicsCrmConnection, entity, entityName, entityIdName)).Select(valueEntity => valueEntity.value);

			return entities.ToList();
		}
	}
}
