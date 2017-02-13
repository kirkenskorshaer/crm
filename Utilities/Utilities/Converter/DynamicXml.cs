using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace Utilities.Converter
{
	public class DynamicXml : DynamicObject
	{
		private XElement _root;
		private DynamicXml(XElement root)
		{
			_root = root;
		}

		public static DynamicXml Parse(string xmlString)
		{
			return new DynamicXml(XDocument.Parse(xmlString).Root);
		}

		public static DynamicXml Load(string filename)
		{
			return new DynamicXml(XDocument.Load(filename).Root);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = null;

			var attribute = _root.Attribute(binder.Name);
			if (attribute != null)
			{
				result = attribute.Value;
				return true;
			}

			var nodes = _root.Elements(binder.Name);
			if (nodes.Count() > 1)
			{
				result = nodes.Select(lNode => lNode.HasElements ? (object)new DynamicXml(lNode) : lNode.Value).ToList();
				return true;
			}

			XElement node = _root.Element(binder.Name);
			if (node != null)
			{
				result = node.HasElements ? (object)new DynamicXml(node) : node.Value;
				return true;
			}

			return true;
		}
	}
}
