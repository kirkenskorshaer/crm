using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SystemInterface.DanskeBank
{
	public class ApplicationRequest
	{
		/// <summary>
		/// Max16Text (min 1, max16)[1..1]
		/// 
		/// Customer User ID registered for the sending customer.
		/// Usually this ID matches the ID in the certificate used to sign the payload (for UploadFile).
		/// See also Signature field of the ApplicationReque st and Appendix E.
		/// </summary>
		public string CustomerId;

		public CommandEnum Command;
		public enum CommandEnum
		{
			UploadFile = 1,
			DownLoadFileList = 2,
			DownloadFile = 4,
		}

		/// <summary>
		/// ISODate [0..1]
		///
		///	When requesting data from the bank, e.g.with the DownloadFileList operation, this element can be used to specify filtering criteria.
		/// This element contains a date which specifies the starting point of the time filter, inclusive.
		/// If this element is not present, but EndDate is given, it means the filtering criteria does not have a starting point.
		/// </summary>
		public DateTime? StartDate;

		/// <summary>
		/// ISODate [0..1]
		/// 
		/// When requesting data from the bank, e.g. with the DownloadFileList operation, this element can be used to specify filtering criteria.
		/// This element contains a date which specifies the ending point of the time filter, inclusive.
		/// If this element is not present, but StartDate is given, it means the filtering criteria does not have an ending point.
		/// </summary>
		public DateTime? EndDate;

		/// <summary>
		/// Max10Text Must specify one of the following:
		///  “NEW” = Give me a list of those files that haven’t been downloaded yet.
		///  “DLD” = Give me a list of those files that have already been downloaded.
		///  “ALL” (default) = Give me a list of both new and already downloaded files
		/// </summary>
		public StatusEnum Status;
		public enum StatusEnum
		{
			NEW = 1,
			DLD = 2,
			ALL = 4,
		}

		//public string ServiceId

		public EnvironmentEnum Environment;
		public enum EnvironmentEnum
		{
			PRODUCTION = 1,
			TEST = 2,
		}

		/// <summary>
		/// [0..1] [1..n]
		/// 
		///  Unique identification of the file that is the target of the operation.
		///  This element is used in operations DownloadFile, DeleteFile and ConfirmFile to specify which file is to be operated upon.
		///  The customer must have obtained the FileReference value beforehand, e. g. using the DownloadFileList or UploadFile operations.
		///  The customer never generates the FileReference.
		///  This value is generated in the bank.
		///  It is comparable to a file system File Handle.
		/// </summary>
		public List<string> FileReferences;

		/// <summary>
		/// Max80Text [0..1]
		///  A name given to the file by the customer.
		///  The value given in this element in the UploadFile operation is stored in the bank and shown in various listings to help the customer identify the file.
		///  Please note that the real identification of a file is the FileReference.
		///  The UserFileName field is just comment type information and is not used by bank systems.
		///  Rule: This element is mandatory in the operation UploadFile and ignored in all other operations.
		///  If missing, request will be rejected, responsecode = “No File Name”
		///  Used Will be displayed in List of files in Business Online
		/// </summary>
		public string UserFileName;

		/// <summary>
		/// Boolean [0. . 1] Compression indicator for the content and compression request for the responses.
		///  Rule: If this element is present and the content is string true (case-sensitive) or 1 it means that the Content is compressed or the requested data should be compressed.
		///  If this element is present and the content is string false (case-sensitive) or 0 it means that the Content is NOT compressed or the requested data should NOT be compressed.
		/// </summary>
		public bool Compression;

		public CompressionMethodEnum? CompressionMethod;
		public enum CompressionMethodEnum
		{
			GZIP = 1,
			DEFLATE = 2,
		}

		/// <summary>
		/// Max80Text [1..1]
		///  This element contains the name and version of the client side software that generated the request.
		///  It is used for customer support purposes.
		///  Can be monitored in the EDI Gateway at Danske Bank.
		/// </summary>
		public string SoftwareId;

		/// <summary>
		/// Max40Text [0..1] Specified the type of file in the request.
		///  Can also be used as a filter in the operation DownloadFileList.
		///  The values accepted in this element are agreed upon between the customer and the bank.
		///  New file types will be added, and they will not affect the schema.
		///  An appendix will be provided listing commonly used FileTypes.
		///  Rule: For ISO messages, the ISO name must be used.
		///  This element is mandatory in operation UploadFile, optional in DownloadFileList, ignored in other operations.
		/// </summary>
		private string FileType = "camt.053.001.02";

		/// <summary>
		/// Base64Binary [0..1] The actual file in the UploadFile operation.
		///  The file is in Base64 format.
		///  Rule: This element is mandatory in operation UploadFile, ignored in other operations.
		/// </summary>
		public byte[] Content;

		internal XDocument ToXDocument()
		{
			XElement ApplicationRequestElement = new XElement("ApplicationRequest");
			XDocument xDocument = new XDocument(ApplicationRequestElement);

			AddIfNotNull(ApplicationRequestElement, "CustomerId", CustomerId);
			AddIfNotNull(ApplicationRequestElement, "Environment", Environment);
			AddIfNotNull(ApplicationRequestElement, "Command", Command);
			AddIfNotNull(ApplicationRequestElement, "StartDate", StartDate);
			AddIfNotNull(ApplicationRequestElement, "EndDate", EndDate);
			AddIfNotNull(ApplicationRequestElement, "Status", Status);
			AddIfNotNull(ApplicationRequestElement, "UserFileName", UserFileName);
			AddIfNotNull(ApplicationRequestElement, "Compression", Compression);
			AddIfNotNull(ApplicationRequestElement, "CompressionMethod", CompressionMethod);
			AddIfNotNull(ApplicationRequestElement, "SoftwareId", SoftwareId);
			AddIfNotNull(ApplicationRequestElement, "FileType", FileType);
			AddIfNotNull(ApplicationRequestElement, "Content", Content);

			XElement FileReferencesElement = new XElement("FileReferences");

			foreach (string fileReference in FileReferences)
			{
				FileReferencesElement.Add(new XElement("FileReference", fileReference));
			}

			ApplicationRequestElement.Add(FileReferencesElement);

			return xDocument;
		}

		private void AddIfNotNull(XElement currentElement, string name, object objectToAdd)
		{
			if (objectToAdd == null)
			{
				return;
			}

			if (objectToAdd is string && string.IsNullOrWhiteSpace((string)objectToAdd))
			{
				return;
			}

			if (objectToAdd is Enum && (int)objectToAdd == 0)
			{
				return;
			}

			currentElement.Add(new XElement(name, objectToAdd));
		}
	}
}
