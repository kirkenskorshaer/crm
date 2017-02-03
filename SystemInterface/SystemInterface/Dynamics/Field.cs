using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace SystemInterface.Dynamics.Crm
{
	public class Field : AbstractCrm
	{
		public string new_name;
		public DateTime createdon;
		public DateTime modifiedon;

		private static readonly ColumnSet ColumnSetField = new ColumnSet(
			"new_fieldid",
			"new_name");

		private static readonly ColumnSet ColumnSetFieldCrmGenerated = new ColumnSet("createdon", "modifiedon");

		public Field(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public Field(IDynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetFieldCrmGenerated; } }

		protected override string entityName { get { return "new_field"; } }
		protected override string idName { get { return "new_fieldid"; } }

		protected override Entity GetAsEntity(bool includeContactId)
		{
			Entity crmEntity = new Entity("field");

			if (includeContactId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_name", new_name));

			return crmEntity;
		}

		public static List<Field> ReadAllFields(IDynamicsCrmConnection dynamicsCrmConnection)
		{
			return StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, new List<string>() { "new_fieldid", "new_entity" }, new Dictionary<string, string>(), null, (connection, entity) => new Field(connection, entity), new PagingInformation());
		}
	}
}
