using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SystemInterface.Dynamics.Crm
{
	public class KeyValueEntity<KeyType, ValueType> : AbstractCrm
	{
		private string _entityName;
		private string _idName;
		public KeyType key;
		public ValueType value;

		public KeyValueEntity(DynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection)
		{
			_idName = idName;
			_entityName = entityName;

			InitializeFromEntity(crmEntity);
		}

		public KeyValueEntity(DynamicsCrmConnection connection) : base(connection)
		{
			throw new NotImplementedException();
		}

		public KeyValueEntity(DynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
			throw new NotImplementedException();
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { throw new NotImplementedException(); } }

		protected override string entityName { get { return _entityName; } }

		protected override string idName { get { return _idName; } }

		protected override CrmEntity GetAsEntity(bool includeId) { throw new NotImplementedException(); }
	}
}
