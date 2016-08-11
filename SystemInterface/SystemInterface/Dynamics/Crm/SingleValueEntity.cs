using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public class SingleValueEntity<ValueType> : AbstractValueEntity
	{
		public ValueType value;

		public SingleValueEntity(DynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection, crmEntity, entityName, idName)
		{
		}
	}
}
