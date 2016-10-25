using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class Byarbejde : AbstractCrm
	{
		public string new_name;
		public DateTime createdon { get; private set; }
		public DateTime modifiedon { get; private set; }

		private static readonly ColumnSet ColumnSetByarbejde = new ColumnSet(
			"new_name",
			"new_byarbejdeid",

			"createdon",
			"modifiedon",
			"modifiedby"
		);

		private static readonly ColumnSet ColumnSetByarbejdeCrmGenerated = new ColumnSet("createdon", "modifiedon", "modifiedby");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetByarbejdeCrmGenerated; } }

		protected override string entityName { get { return "new_byarbejde"; } }
		protected override string idName { get { return "new_byarbejdeid"; } }

		public Byarbejde(IDynamicsCrmConnection dynamicsCrmConnection) : base(dynamicsCrmConnection)
		{
		}

		public Byarbejde(IDynamicsCrmConnection dynamicsCrmConnection, Entity entity) : base(dynamicsCrmConnection, entity)
		{
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_name", new_name));

			return crmEntity;
		}

		public static Byarbejde Read(IDynamicsCrmConnection dynamicsCrmConnection, Guid byarbejdeid)
		{
			Entity contactEntity = dynamicsCrmConnection.Service.Retrieve("new_byarbejde", byarbejdeid, ColumnSetByarbejde);

			Byarbejde byarbejde = new Byarbejde(dynamicsCrmConnection, contactEntity);

			return byarbejde;
		}

		public static List<Byarbejde> Read(IDynamicsCrmConnection connection, string name)
		{
			ConditionExpression equalsNameExpression = new ConditionExpression
			{
				AttributeName = "new_name",
				Operator = ConditionOperator.Equal,
			};
			equalsNameExpression.Values.Add(name);

			FilterExpression filterExpression = new FilterExpression();
			filterExpression.Conditions.Add(equalsNameExpression);

			QueryExpression query = new QueryExpression("new_byarbejde")
			{
				ColumnSet = ColumnSetByarbejde,
			};
			query.Criteria.AddFilter(filterExpression);

			EntityCollection ByarbejdeEntities = connection.Service.RetrieveMultiple(query);

			List<Byarbejde> byarbejder = ByarbejdeEntities.Entities.Select(entity => new Byarbejde(connection, entity)).ToList();

			return byarbejder;
		}

		public static void WriteIndbetalingsum(IDynamicsCrmConnection dynamicsCrmConnection, Guid id, decimal amount)
		{
			Update(dynamicsCrmConnection, "new_byarbejde", "new_byarbejdeid", id, new Dictionary<string, object>()
			{
				{ "new_indbetalingsum", new Money(amount) }
			});
		}
	}
}
