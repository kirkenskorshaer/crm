using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			}

			EntityCollection entityCollection = connection.Service.RetrieveMultiple(query);

			List<AbstractCrmType> crmEntities = entityCollection.Entities.Select(entity => CrmTypeConstructor(connection, entity)).ToList();

			return crmEntities;
		}
	}
}
