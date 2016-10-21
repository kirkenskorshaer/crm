using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SystemInterface.DanskeBank
{
	public class TrippleDESDocumentEncryption
	{
		protected XmlDocument docValue;
		protected TripleDES tripleDES;
		private string kkKey = "kkKey";

		public TrippleDESDocumentEncryption(XmlDocument Doc)
		{
			tripleDES = new TripleDESCryptoServiceProvider();

			if (Doc != null)
			{
				docValue = Doc;
			}
			else
			{
				throw new ArgumentNullException("Doc");
			}
		}

		//public XmlDocument Doc { set { docValue = value; } get { return docValue; } }
		//public TripleDES Alg { set { tripleDES = value; } get { return tripleDES; } }

		public void Clear()
		{
			if (tripleDES != null)
			{
				tripleDES.Clear();
			}
			else
			{
				throw new Exception("No TripleDES key was found to clear.");
			}
		}

		public void Encrypt(string Element, X509Certificate2 certificate, bool includeKey)
		{
			// Find the element by name and create a new
			// XmlElement object.
			XmlElement inputElement = docValue.GetElementsByTagName(Element)[0] as XmlElement;

			// If the element was not found, throw an exception.
			if (inputElement == null)
			{
				throw new Exception("The element was not found.");
			}

			// Create a new EncryptedXml object.
			EncryptedXml exml = new EncryptedXml(docValue);

			// Encrypt the element using the symmetric key.
			byte[] rgbOutput = exml.EncryptData(inputElement, tripleDES, false);

			// Create an EncryptedData object and populate it.
			EncryptedData ed = new EncryptedData();

			// Specify the namespace URI for XML encryption elements.
			ed.Type = EncryptedXml.XmlEncElementUrl;

			// Specify the namespace URI for the TrippleDES algorithm.
			ed.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncTripleDESUrl);

			// Create a CipherData element.
			ed.CipherData = new CipherData();

			// Set the CipherData element to the value of the encrypted XML element.
			ed.CipherData.CipherValue = rgbOutput;

			// Encrypt the session key and add it to an EncryptedKey element.
			EncryptedKey encryptedKey = new EncryptedKey();

			RSACryptoServiceProvider publicKeyProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
			byte[] encryptedKeyBytes = EncryptedXml.EncryptKey(tripleDES.Key, publicKeyProvider, false);

			encryptedKey.CipherData = new CipherData(encryptedKeyBytes);

			encryptedKey.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);

			// Create a new KeyInfo element.
			ed.KeyInfo = new KeyInfo();

			if (includeKey)
			{
				// Create a new KeyInfoName element.
				KeyInfoName keyInfoName = new KeyInfoName(kkKey);

				// Add the KeyInfoName element to the 
				// EncryptedKey object.
				encryptedKey.KeyInfo.AddClause(keyInfoName);
			}

			KeyInfoX509Data keyInfoData = new KeyInfoX509Data(certificate);
			ed.KeyInfo.AddClause(keyInfoData);

			ed.KeyInfo.AddClause(new KeyInfoEncryptedKey(encryptedKey));

			// Replace the plaintext XML elemnt with an EncryptedData element.
			EncryptedXml.ReplaceElement(inputElement, ed, false);
		}

		public void Decrypt(X509Certificate2 certificate, bool useFakeKeyName)
		{
			if (useFakeKeyName)
			{
				KeyInfoName keyInfoName = new KeyInfoName(kkKey);

				XmlNodeList encryptedKeyNodes = docValue.GetElementsByTagName("EncryptedKey");
				XmlNode encryptedKeyNode = encryptedKeyNodes.Item(0);
				XmlNode referenceNode = encryptedKeyNode.FirstChild;

				XmlElement infoElement = docValue.CreateElement(null, "KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
				XmlElement keyNameElement = docValue.CreateElement(null, "KeyName", "http://www.w3.org/2000/09/xmldsig#");
				keyNameElement.InnerText = kkKey;
				infoElement.AppendChild(keyNameElement);

				encryptedKeyNode.InsertAfter(infoElement, referenceNode);

				encryptedKeyNode.ParentNode.AppendChild(encryptedKeyNode);
			}

			EncryptedXml exml = new EncryptedXml(docValue);

			RSACryptoServiceProvider privateKeyProvider = (RSACryptoServiceProvider)certificate.PrivateKey;
			exml.AddKeyNameMapping(kkKey, privateKeyProvider);

			exml.DecryptDocument();
		}
	}
}
