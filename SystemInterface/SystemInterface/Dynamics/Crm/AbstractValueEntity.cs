using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SystemInterface.Dynamics.Crm
{
	public abstract class AbstractValueEntity : AbstractCrm
	{
		private string _entityName;
		private string _idName;

		public AbstractValueEntity(IDynamicsCrmConnection connection, Entity crmEntity, string entityName, string idName) : base(connection)
		{
			_idName = idName;
			_entityName = entityName;

			InitializeFromEntity(crmEntity);
		}

		public AbstractValueEntity(IDynamicsCrmConnection connection, string entityName, string idName) : base(connection)
		{
			_idName = idName;
			_entityName = entityName;
		}

		public AbstractValueEntity(IDynamicsCrmConnection connection) : base(connection)
		{
			throw new NotImplementedException();
		}

		public AbstractValueEntity(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
			throw new NotImplementedException();
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { throw new NotImplementedException(); } }

		protected override string entityName { get { return _entityName; } }

		protected override string idName { get { return _idName; } }

		protected override CrmEntity GetAsEntity(bool includeId) { throw new NotImplementedException(); }
	}
}
