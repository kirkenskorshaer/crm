using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class View : AbstractCrm
	{
		public Guid savedqueryid;
        public string name;
		//public XDocument columnsetxml;
		//public XDocument fetchxml;
		//public XDocument layoutxml;
		public string columnsetxml;
		public string fetchxml;
		public string layoutxml;
		private int returnedtypecode = 1;
		private int querytype = 0;
		//private string advancedgroupby = "versionnumber";

		private static readonly ColumnSet ColumnSetView = new ColumnSet(
			"savedqueryid",
			"name",
			"columnsetxml",
			"fetchxml",
			"layoutxml",
			"returnedtypecode",
			"querytype"//,
			//"advancedgroupby"
		);

		private static readonly ColumnSet ColumnSetViewCrmGenerated = new ColumnSet("savedqueryid");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetViewCrmGenerated; } }

		public View(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public View(DynamicsCrmConnection connection, Entity crmEntity) : base(connection, crmEntity)
		{
		}

		protected override string entityName { get { return "savedquery"; } }
		protected override string idName { get { return "savedqueryid"; } }

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("name", name));

			if (columnsetxml != null)
			{
				//crmEntity.Attributes.Add(new KeyValuePair<string, object>("columnsetxml", columnsetxml.ToString()));
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("columnsetxml", columnsetxml));
			}

			if (fetchxml != null)
			{
				//crmEntity.Attributes.Add(new KeyValuePair<string, object>("fetchxml", fetchxml.ToString()));
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("fetchxml", fetchxml));
			}

			if (layoutxml != null)
			{
				//crmEntity.Attributes.Add(new KeyValuePair<string, object>("layoutxml", layoutxml.ToString()));
				crmEntity.Attributes.Add(new KeyValuePair<string, object>("layoutxml", layoutxml));
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("returnedtypecode", returnedtypecode));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("querytype", querytype));
			//crmEntity.Attributes.Add(new KeyValuePair<string, object>("advancedgroupby", advancedgroupby));

			return crmEntity;
		}

		public static void MaintainView(DynamicsCrmConnection dynamicsCrmConnection)
		{
			View view = new View(dynamicsCrmConnection);

			//view.fetchxml = XDocument.Load("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Dynamics/Crm/FetchXml/AccountAntalIndsamlingshjaelpere.xml");
			//view.layoutxml = XDocument.Load("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Dynamics/Crm/ViewXml/AccountAntalIndsamlingshjaelpereView.xml");
			//view.columnsetxml = XDocument.Load("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Dynamics/Crm/ViewXml/AccountAntalIndsamlingshjaelpereView.xml");
			//view.columnsetxml = XDocument.Load("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Dynamics/Crm/ViewXml/Test.xml");

			//XDocument allXml = XDocument.Load("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Dynamics/Crm/ViewXml/AccountAntalIndsamlingshjaelpereView.xml");
			XDocument allXml = XDocument.Load("C:/Users/Svend/Documents/GitHub/crm/SystemInterface/SystemInterface/Dynamics/Crm/ViewXml/Test.xml");

			view.fetchxml = allXml.Element("savedquery").Element("fetchxml").Element("fetch").ToString();

			view.columnsetxml = allXml.Element("savedquery").Element("columnsetxml").ToString();

			view.name = $"testView {DateTime.Now.ToString("HH:mm")}";
			
			//FetchExpression fetchExpression = new FetchExpression(fetchXml.ToString());

			//EntityCollection entityCollection = dynamicsCrmConnection.Service.RetrieveMultiple(fetchExpression);

			view.Insert();

			/*
			foreach (Entity entity in entityCollection.Entities)
            {
				foreach (KeyValuePair<string, object> attribute in entity.Attributes)
				{
					Console.Out.WriteLine($"{attribute.Key} = {((AliasedValue)attribute.Value).Value}");
				}
				Console.Out.WriteLine("-----------------------------------------");
			}
			*/
		}
	}
}
