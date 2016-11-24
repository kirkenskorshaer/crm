using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class SavedQueryTest : TestBase
	{
		[Test]
		public void SavedQueryCanBeCreated()
		{
			SystemUser user = SystemUser.ReadByDomainname(_dynamicsCrmConnection, "kad\\crmAdmin");

			string fetchXml = GetFetchXml();
			string layoutXml = GetLayoutXml();

			SavedQuery savedQuery = SavedQuery.CreateAndInsert(_dynamicsCrmConnection, $"test {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", "account", fetchXml, layoutXml, user.Id);
		}

		private string GetLayoutXml()
		{
			XDocument layoutXmlDocument = new XDocument
			(
				new XElement("grid", new XAttribute("name", "resultset"), new XAttribute("object", "3"), new XAttribute("jump", "name"), new XAttribute("select", "1"), new XAttribute("preview", "1"), new XAttribute("icon", "1"),
					new XElement("row", new XAttribute("name", "result"), new XAttribute("id", "accountid"),
						new XElement("cell", new XAttribute("name", "name"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "accountid"), new XAttribute("width", "150"))
						/*,

						new XElement("cell", new XAttribute("name", "address1_line1"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "address1_city"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "new_forventetantalindsamleregruppe2016"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "new_forventetantalindsamlere2016"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "new_omraadekoordinatorid"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "new_indsamlingskoordinatorid"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "new_bykoordinatorid"), new XAttribute("width", "150")),
						new XElement("cell", new XAttribute("name", "new_forventetantalindsamlere2016"), new XAttribute("width", "150"))
						*/
						//,
						//new XElement("cell", new XAttribute("name", "Indsamlere"), new XAttribute("width", "150"))
					)
				)
			);

			return layoutXmlDocument.ToString();
		}

		private string GetFetchXml()
		{
			XDocument fetchXmlDocument = new XDocument
			(
				new XElement("fetch", new XAttribute("version", "1.0"), new XAttribute("aggregate", "true"),
					new XElement("entity", new XAttribute("name", "account"),
						new XElement("attribute", new XAttribute("name", "name"), new XAttribute("alias", "name"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "accountid"), new XAttribute("alias", "accountid"), new XAttribute("groupby", "true"))
						/*,
						new XElement("attribute", new XAttribute("name", "address1_line1"), new XAttribute("alias", "address1_line1"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "address1_city"), new XAttribute("alias", "address1_city"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "new_forventetantalindsamleregruppe2016"), new XAttribute("alias", "new_forventetantalindsamleregruppe2016"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "new_forventetantalindsamlere2016"), new XAttribute("alias", "new_forventetantalindsamlere2016"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "new_omraadekoordinatorid"), new XAttribute("alias", "new_omraadekoordinatorid"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "new_indsamlingskoordinatorid"), new XAttribute("alias", "new_indsamlingskoordinatorid"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "new_bykoordinatorid"), new XAttribute("alias", "new_bykoordinatorid"), new XAttribute("groupby", "true")),
						new XElement("order", new XAttribute("alias", "name"), new XAttribute("descending", "false")),
						new XElement("filter", new XAttribute("type", "and"),
							new XElement("condition", new XAttribute("attribute", "new_erindsamlingssted"), new XAttribute("operator", "eq"), new XAttribute("value", "100000000"))
						)
						*/
						/*,
						new XElement("link-entity", new XAttribute("name", "contact"), new XAttribute("from", "new_indsamler2016"), new XAttribute("to", "accountid"), new XAttribute("link-type", "inner"),
							new XElement("attribute", new XAttribute("name", "contactid"), new XAttribute("alias", "Indsamlere"), new XAttribute("aggregate", "count"))
						)*/
					)
				)
			);

			return fetchXmlDocument.ToString();
		}
	}
}
