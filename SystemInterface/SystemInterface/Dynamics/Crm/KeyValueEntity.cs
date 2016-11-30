using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class KeyValueEntity<KeyType, ValueType> : AbstractValueEntity
	{
		public KeyType key;
		public ValueType value;

		public KeyValueEntity(IDynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection, crmEntity, entityName, idName)
		{
		}

		public static List<KeyValuePair<KeyType, ValueType>> GetAll(IDynamicsCrmConnection dynamicsCrmConnection, XDocument xDocument, string entityName, string entityIdName)
		{
			List<KeyValueEntity<KeyType, ValueType>> keyValueEntities = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (LDynamicsCrmConnection, entity) => new KeyValueEntity<KeyType, ValueType>(dynamicsCrmConnection, entity, entityName, entityIdName), new PagingInformation());

			return keyValueEntities.Select(keyValue => new KeyValuePair<KeyType, ValueType>(keyValue.key, keyValue.value)).ToList();
		}
	}
}
