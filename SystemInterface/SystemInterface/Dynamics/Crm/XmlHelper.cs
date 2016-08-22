using System.Linq;
using System.Xml.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public static class XmlHelper
	{
		public static void RemoveAllAttributes(XDocument xDocument)
		{
			xDocument.Descendants().Where(descendant => descendant.Name == "attribute").Remove();
		}

		public static void AddAliasedValue(XDocument xDocument, string crmName, string aliasName)
		{
			xDocument.Element("fetch").Element("entity").Add(new XElement("attribute", new XAttribute("name", crmName), new XAttribute("alias", aliasName)));
		}

		public static void AddCondition(XDocument xDocument, string fieldName, string operatorString, string valueString = null)
		{
			XElement filterElement = xDocument.Element("fetch").Element("entity").Element("filter");

			if (filterElement == null)
			{
				xDocument.Element("fetch").Element("entity").Add(new XElement("filter"));
			}

			if (valueString == null)
			{
				xDocument.Element("fetch").Element("entity").Element("filter").Add(new XElement("condition", new XAttribute("attribute", fieldName), new XAttribute("operator", operatorString)));
			}
			else
			{
				xDocument.Element("fetch").Element("entity").Element("filter").Add(new XElement("condition", new XAttribute("attribute", fieldName), new XAttribute("operator", operatorString), new XAttribute("value", valueString)));
			}
		}

		public static void SetCount(XDocument xDocument, int count)
		{
			if (xDocument.Element("fetch").Attribute("count") == null)
			{
				xDocument.Element("fetch").Add(new XAttribute("count", count));
			}
			else
			{
				xDocument.Element("fetch").Attribute("count").Value = count.ToString();
			}
		}
	}
}
