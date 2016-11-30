using DataLayer;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using DatabaseAggregateSubTable = DataLayer.MongoData.Option.Options.Logic.AggregateSubTable;
using System;
using System.Xml.Linq;

namespace Administration.Option.Options.Logic
{
	public class AggregateSubTable : AbstractReportingDataOptionBase
	{
		private DatabaseAggregateSubTable _databaseAggregateSubTable;

		public AggregateSubTable(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseAggregateSubTable = (DatabaseAggregateSubTable)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			SetDynamicsCrmConnectionIfEmpty();

			DatabaseAggregateSubTable.AggregateEnum aggregate = _databaseAggregateSubTable.aggregate;
			string topEntityName = _databaseAggregateSubTable.topEntityName;
			string topEntityIdName = _databaseAggregateSubTable.topEntityIdName;
			string topEntityResultFieldName = _databaseAggregateSubTable.topEntityResultFieldName;
			string topEntityReferenceIdName = _databaseAggregateSubTable.topEntityReferenceIdName;

			string aggregateEntityName = _databaseAggregateSubTable.aggregateEntityName;
			string aggregateEntityIdName = _databaseAggregateSubTable.aggregateEntityIdName;
			string aggregateFieldName = _databaseAggregateSubTable.aggregateFieldName;
			string aggregateReferenceIdName = _databaseAggregateSubTable.aggregateReferenceIdName;

			List<KeyValuePair<Guid, object>> topRows = GetTopFields(topEntityName, topEntityReferenceIdName, topEntityResultFieldName);

			report.Workload = topRows.Count;

			foreach (KeyValuePair<Guid, object> row in topRows)
			{
				object aggregateValue = GetAggregateValue(aggregateEntityName, aggregateEntityIdName, aggregateReferenceIdName, topEntityReferenceIdName, row.Key, aggregateFieldName, aggregate);

				if (aggregateValue.Equals(row.Value) == false)
				{
					report.SubWorkload++;

					UpdateRow(topEntityName, topEntityIdName, row.Key, topEntityResultFieldName, aggregateValue);
				}
			}

			report.Success = true;
		}

		private void UpdateRow(string topEntityName, string topEntityIdName, Guid key, string resultFieldName, object aggregateValue)
		{
			AbstractCrm.Update(_dynamicsCrmConnection, topEntityName, topEntityIdName, key, new Dictionary<string, object>()
			{
				{ resultFieldName, aggregateValue }
			});
		}

		private List<KeyValuePair<Guid, object>> GetTopFields(string topEntityName, string topEntityReferenceIdName, string resultFieldName)
		{
			XDocument xDocument = new XDocument
			(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", topEntityName),
						new XElement("attribute", new XAttribute("name", topEntityReferenceIdName), new XAttribute("alias", "key")),
						new XElement("attribute", new XAttribute("name", resultFieldName), new XAttribute("alias", "value"))
					)
				)
			);


			List<KeyValuePair<Guid, object>> resultById = KeyValueEntity<Guid, object>.GetAll(_dynamicsCrmConnection, xDocument, topEntityName, topEntityReferenceIdName);

			return resultById;
		}

		private object GetAggregateValue(string aggregateEntityName, string aggregateEntityIdName, string aggregateReferenceName, string topEntityReferenceName, object topEntityReferenceValue, string aggregateFieldName, DatabaseAggregateSubTable.AggregateEnum aggregate)
		{
			XDocument xDocument = new XDocument
			(
				new XElement("fetch", new XAttribute("aggregate", "true"),
					new XElement("entity", new XAttribute("name", aggregateEntityName),
						new XElement("attribute", new XAttribute("name", aggregateFieldName), new XAttribute("alias", "value"), new XAttribute("aggregate", aggregate))
					)
				)
			);

			XmlHelper.AddCondition(xDocument, aggregateReferenceName, "eq", topEntityReferenceValue.ToString());

			List<object> objects = SingleValueEntity<object>.GetAll(_dynamicsCrmConnection, xDocument, aggregateEntityName, aggregateEntityIdName);

			return objects.SingleOrDefault();
		}

		public static List<AggregateSubTable> Find(MongoConnection connection)
		{
			List<DatabaseAggregateSubTable> options = DatabaseAggregateSubTable.ReadAllowed<DatabaseAggregateSubTable>(connection);

			return options.Select(option => new AggregateSubTable(connection, option)).ToList();
		}
	}
}
