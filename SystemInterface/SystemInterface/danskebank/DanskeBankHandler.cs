using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SystemInterface.DanskeBankEdiService;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace SystemInterface.DanskeBank
{
	public class DanskeBankHandler
	{
		private CorporateFileServicePortTypeClient _client;
		private X509Certificate2 _certificate;
		private string _customerUserId;
		public static ApplicationRequest.EnvironmentEnum Environment = ApplicationRequest.EnvironmentEnum.TEST;

		public DanskeBankHandler(string customerUserId, X509Certificate2 certificate)
		{
			_customerUserId = customerUserId;
			_certificate = certificate;

			Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
			//EndpointAddress endpointAddress = new EndpointAddress("https://businessws.danskebank.com/financialservice/edifileservice.asmx");
			EndpointAddress endpointAddress = new EndpointAddress("https://localhost/TestService.asmx");

			_client = new CorporateFileServicePortTypeClient(binding, endpointAddress);
		}

		public XDocument DownloadNextIso20022(string fileReference, string softwareId)
		{
			string[] files = DownloadFileList();

			List<string> filesSortedByDate = files.OrderBy(file => file).ToList();

			string firstFileName = filesSortedByDate.First();

			XDocument iso20022File = DownloadIso20022(fileReference, softwareId);//firstFileName

			return iso20022File;
		}

		public XDocument DownloadIso20022(string fileReference, string softwareId)
		{
			DownloadFileRequest downloadFileRequest = new DownloadFileRequest();
			downloadFileRequest.ApplicationRequest = GetDownloadFileApplicationRequest(fileReference, softwareId);
			downloadFileRequest.RequestHeader = GetRequestHeader();

			DownloadFileResponse downloadFileResponse = _client.downloadFile(downloadFileRequest);

			VerifyHeadersAfterResponse(downloadFileRequest.RequestHeader, downloadFileResponse.ResponseHeader);

			XDocument file = DecryptAndUncompressDownloadFileResponse(downloadFileResponse.ApplicationResponse);

			return file;
		}

		public XDocument EncryptApplicationRequest(X509Certificate2 certificate, XDocument xDocument)
		{
			EncryptedXml eXml = new EncryptedXml();

			XmlDocument xmlDocument = new XmlDocument();
			using (XmlReader xmlReader = xDocument.CreateReader())
			{
				xmlDocument.Load(xmlReader);
			}

			TrippleDESDocumentEncryption xmlTDES = new TrippleDESDocumentEncryption(xmlDocument);

			xmlTDES.Encrypt("ApplicationRequest", _certificate, true);

			//return xmlTDES.Doc.OuterXml;

			using (var nodeReader = new XmlNodeReader(xmlDocument))
			{
				nodeReader.MoveToContent();
				xDocument = XDocument.Load(nodeReader);
			}

			return xDocument;

			//byte[] encryptedElement = eXml.EncryptData(xmlDocument.DocumentElement, tDESkey, false);
		}

		public XDocument DecryptApplicationRequest(X509Certificate2 certificate, XDocument xDocument)
		{
			EncryptedXml eXml = new EncryptedXml();

			XmlDocument xmlDocument = new XmlDocument();
			using (XmlReader xmlReader = xDocument.CreateReader())
			{
				xmlDocument.Load(xmlReader);
			}

			TrippleDESDocumentEncryption xmlTDES = new TrippleDESDocumentEncryption(xmlDocument);

			xmlTDES.Decrypt(_certificate, false);

			//return xmlTDES.Doc.OuterXml;
			using (var nodeReader = new XmlNodeReader(xmlDocument))
			{
				nodeReader.MoveToContent();
				xDocument = XDocument.Load(nodeReader);
			}

			return xDocument;
			//byte[] encryptedElement = eXml.EncryptData(xmlDocument.DocumentElement, tDESkey, false);
		}

		private void VerifyHeadersAfterResponse(RequestHeader requestHeader, ResponseHeader responseHeader)
		{
			if (requestHeader.SenderId != responseHeader.SenderId)
			{
				throw new Exception($"request sender id {requestHeader.SenderId} did not match {responseHeader.SenderId}");
			}

			if (requestHeader.RequestId != responseHeader.RequestId)
			{
				throw new Exception($"request request id {requestHeader.SenderId} did not match {responseHeader.SenderId}");
			}

			int responseCodeInt;
			bool isValidCode = int.TryParse(responseHeader.ResponseCode, out responseCodeInt);

			if (isValidCode == false)
			{
				throw new Exception($"invalid code: {responseHeader.ResponseCode} with text: {responseHeader.ResponseText}");
			}

			ResponseCode.ResponseCodeEnum response = ResponseCode.GetResponse(responseHeader.ResponseCode);

			if (response != ResponseCode.ResponseCodeEnum.OK)
			{
				throw new Exception($"error code: {responseHeader.ResponseCode} text: {responseHeader.ResponseText}");
			}
		}

		public string[] DownloadFileList()
		{
			DownloadFileListRequest downloadFileListRequest = new DownloadFileListRequest();
			downloadFileListRequest.RequestHeader = GetRequestHeader();
			downloadFileListRequest.ApplicationRequest = GetDownloadFileListApplicationRequest();

			DownloadFileListResponse downloadFileListResponse = _client.downloadFileList(downloadFileListRequest);

			VerifyHeadersAfterResponse(downloadFileListRequest.RequestHeader, downloadFileListResponse.ResponseHeader);

			string[] fileList = DecryptAndUncompressDownloadFileListResponse(downloadFileListResponse.ApplicationResponse);

			return fileList;
		}

		private string[] DecryptAndUncompressDownloadFileListResponse(byte[] applicationResponse)
		{
			throw new NotImplementedException();
		}

		private byte[] GetDownloadFileListApplicationRequest()
		{
			throw new NotImplementedException();
		}

		private XDocument DecryptAndUncompressDownloadFileResponse(byte[] applicationResponse)
		{
			throw new NotImplementedException();
		}

		private RequestHeader GetRequestHeader()
		{
			Guid requestId = Guid.NewGuid();

			return new RequestHeader()
			{
				RequestId = requestId.ToString(),
				SenderId = _customerUserId,
			};
		}

		private byte[] GetDownloadFileApplicationRequest(string fileReference, string softwareId)
		{
			XDocument document = GetDownloadFileApplicationRequestXml(fileReference, softwareId);

			string xml = document.ToString();

			//return Convert.ToBase64String(plainTextBytes);

			byte[] encodedBytes = System.Text.Encoding.UTF8.GetBytes(xml);

			return encodedBytes;
		}

		public XDocument GetDownloadFileApplicationRequestXml(string fileReference, string softwareId)
		{
			ApplicationRequest applicationRequest = new ApplicationRequest()
			{
				CustomerId = _customerUserId,
				Environment = Environment,
				FileReferences = new List<string>() { fileReference },
				Compression = true,
				CompressionMethod = ApplicationRequest.CompressionMethodEnum.GZIP,
				SoftwareId = softwareId,
				Command = ApplicationRequest.CommandEnum.DownloadFile,
			};

			XDocument applicationRequestDocument = applicationRequest.ToXDocument();

			applicationRequestDocument = SignXml(applicationRequestDocument);

			return applicationRequestDocument;
		}

		public void EncryptXml(XElement applicationRequest)
		{

		}

		private XDocument SignXml(XDocument xDocument)
		{
			XmlDocument xmlDocument = new XmlDocument();
			using (XmlReader xmlReader = xDocument.CreateReader())
			{
				xmlDocument.Load(xmlReader);
			}

			SignedXml signedXml = new SignedXml(xmlDocument);
			signedXml.SigningKey = _certificate.PrivateKey;

			Reference reference = new Reference();
			reference.Uri = "";

			XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
			reference.AddTransform(env);

			signedXml.AddReference(reference);
			signedXml.ComputeSignature();

			KeyInfo keyInfo = new KeyInfo();
			KeyInfoX509Data keyInfoData = new KeyInfoX509Data(_certificate);
			keyInfo.AddClause(keyInfoData);
			signedXml.KeyInfo = keyInfo;

			XmlElement xmlDigitalSignature = signedXml.GetXml();
			xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));

			using (var nodeReader = new XmlNodeReader(xmlDocument))
			{
				nodeReader.MoveToContent();
				xDocument = XDocument.Load(nodeReader);
			}

			return xDocument;
		}
	}
}
