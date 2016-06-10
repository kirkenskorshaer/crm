using System;
using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class MaterialeBehovDefinition : AbstractCrm
	{
		public string new_name;
        public int? new_antal;

		public OptionSetValue new_behovtype;
		public behovtypeEnum? behovtype { get { return GetOptionSet<behovtypeEnum>(new_behovtype); } set { new_behovtype = SetOptionSet((int?)value); } }

		public EntityReference new_materialeid;
		public Guid? materialeid { get { return GetEntityReferenceId(new_materialeid); } set { new_materialeid = SetEntityReferenceId(value, "new_materiale"); } }

		private static readonly ColumnSet ColumnSetMaterialeBehovDefinitionr = new ColumnSet(
			"new_name",
            "new_antal",
			"new_behovtype",
			"new_materialeid"
		);

		private static readonly ColumnSet ColumnSetMaterialeBehovDefinitionCrmGenerated = new ColumnSet("createdon");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetMaterialeBehovDefinitionCrmGenerated; } }

		protected override string entityName { get { return "new_materialebehovdefinition"; } }
		protected override string idName { get { return "new_materialebehovdefinitionid"; } }

		public MaterialeBehovDefinition(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public MaterialeBehovDefinition(DynamicsCrmConnection connection, Entity entity) : base(connection, entity)
		{
		}

		public enum behovtypeEnum
		{
			Indsamlingssted = 100000000,
			ForventetAntalIndsamlere2016 = 100000001,
			Indsamlingshjaelper = 100000002,
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
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_antal", new_antal));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_behovtype", new_behovtype));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("new_materialeid", new_materialeid));

			return crmEntity;
		}

		public static List<MaterialeBehovDefinition> FindMaterialeBehovDefinitionPerMateriale(DynamicsCrmConnection dynamicsCrmConnection, Guid materialeId, Func<string, string> getResourcePath, PagingInformation pagingInformation)
		{
			string path = getResourcePath("Dynamics/Crm/FetchXml/MaterialeBehovDefinition/FindMaterialeBehovDefinitionPerMateriale.xml");

			XDocument xDocument = XDocument.Load(path);

			xDocument.Element("fetch").Element("entity").Element("filter").Element("condition").Attribute("value").Value = materialeId.ToString();

			List<MaterialeBehovDefinition> definitioner = StaticCrm.ReadFromFetchXml(dynamicsCrmConnection, xDocument, (connection, lEntity) => new MaterialeBehovDefinition(connection, lEntity), pagingInformation);

			return definitioner;
		}
	}
}
