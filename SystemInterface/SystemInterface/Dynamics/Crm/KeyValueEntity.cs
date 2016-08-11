using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public class KeyValueEntity<KeyType, ValueType> : AbstractValueEntity
	{
		public KeyType key;
		public ValueType value;

		public KeyValueEntity(DynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection, crmEntity, entityName, idName)
		{
		}
	}
}
