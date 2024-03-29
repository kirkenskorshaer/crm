﻿using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public class MaterialePakke : AbstractCrm
	{
		public string new_name;
        public int? new_stoerrelse;

		public EntityReference new_materialeid;
		public Guid? materialeid { get { return GetEntityReferenceId(new_materialeid); } set { new_materialeid = SetEntityReferenceId(value, "new_materiale"); } }

		private static readonly ColumnSet ColumnSetMaterialePakker = new ColumnSet(
			"new_name",
			"new_stoerrelse",
			"new_materialeid"
		);

		private static readonly ColumnSet ColumnSetMaterialePakkeCrmGenerated = new ColumnSet("createdon");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetMaterialePakkeCrmGenerated; } }

		protected override string entityName { get { return "new_materialepakke"; } }
		protected override string idName { get { return "new_materialepakkeid"; } }

		public MaterialePakke(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public MaterialePakke(IDynamicsCrmConnection connection, Entity entity) : base(connection, entity)
		{
		}

		protected override Entity GetAsEntity(bool includeId)
		{
			Entity crmEntity = new Entity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

            crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_name", new_name));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_stoerrelse", new_stoerrelse));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_materialeid", new_materialeid));

			return crmEntity;
		}
	}
}
