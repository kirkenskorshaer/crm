using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SystemInterface.Unwire
{
	public class UnwireConnection : IUnwireConnection
	{
		private string _schemaPath;

		public UnwireConnection(string schemaPath)
		{
			_schemaPath = schemaPath;
		}

		//public void Send()
		public XDocument CreateSendXml(List<Sms> smsList)
		{
			XDocument xDocument = new XDocument();

			ValidateXml(xDocument);

			return xDocument;
		}

		private void ValidateXml(XDocument xDocument)
		{
			XmlSchemaSet schemaSet = new XmlSchemaSet();

			XmlReader xmlSchemaReader = XmlReader.Create(_schemaPath);

			schemaSet.Add(null, xmlSchemaReader);

			xDocument.Validate(schemaSet, schemaValidation);
		}

		private void schemaValidation(object sender, ValidationEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
