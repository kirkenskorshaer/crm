using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public class MaterialeBehov : AbstractCrm
	{
		public OptionSetValue new_forsendelsestatus;
		public forsendelsestatusEnum? forsendelsestatus { get { return GetOptionSet<forsendelsestatusEnum>(new_forsendelsestatus); } set { new_forsendelsestatus = SetOptionSet((int?)value); } }

		public string new_name;
        public int? new_antal;

		public EntityReference new_kontaktpersonid;
		public Guid? kontaktpersonid { get { return GetEntityReferenceId(new_kontaktpersonid); } set { new_kontaktpersonid = SetEntityReferenceId(value, "contact"); } }

		public EntityReference new_materialepakkeid;
		public Guid? materialepakkeid { get { return GetEntityReferenceId(new_materialepakkeid); } set { new_materialepakkeid = SetEntityReferenceId(value, "contact"); } }

		public EntityReference new_modtagerid;
		public Guid? modtagerid { get { return GetEntityReferenceId(new_modtagerid); } set { new_modtagerid = SetEntityReferenceId(value, "account"); } }

		public EntityReference new_leveringsstedid;
		public Guid? leveringsstedid { get { return GetEntityReferenceId(new_leveringsstedid); } set { new_leveringsstedid = SetEntityReferenceId(value, "account"); } }

		private static readonly ColumnSet ColumnSetMaterialeBehov = new ColumnSet(
			"new_name",
			"new_forsendelsestatus",
			"new_antal",
			"new_kontaktpersonid",
			"new_materialepakkeid",
			"new_modtagerid",
			"new_leveringsstedid"
		);

		private static readonly ColumnSet ColumnSetMaterialeBehovCrmGenerated = new ColumnSet("createdon");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetMaterialeBehovCrmGenerated; } }

		protected override string entityName { get { return "new_materialebehov"; } }
		protected override string idName { get { return "new_materialebehovid"; } }

		public MaterialeBehov(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public MaterialeBehov(DynamicsCrmConnection connection, Entity entity) : base(connection, entity)
		{
		}

		public enum forsendelsestatusEnum
		{
			Oprettet = 100000000,
			Afsendt = 100000001,
			Annulleret = 100000002,
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
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_forsendelsestatus", new_forsendelsestatus));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_antal", new_antal));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_kontaktpersonid", new_kontaktpersonid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_materialepakkeid", new_materialepakkeid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_modtagerid", new_modtagerid));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_leveringsstedid", new_leveringsstedid));

			return crmEntity;
		}
	}
}
