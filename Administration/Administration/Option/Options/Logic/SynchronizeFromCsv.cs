using Administration.Mapping.Contact;
using Administration.Option.Options.Data;
using DataLayer;
using DataLayer.SqlData.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using DatabaseSynchronizeFromCsv = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCsv;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Logic
{
	public class SynchronizeFromCsv : AbstractDataOptionBase
	{
		public SynchronizeFromCsv(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSynchronizeFromCsv = (DatabaseSynchronizeFromCsv)databaseOption;
		}

		private DatabaseSynchronizeFromCsv _databaseSynchronizeFromCsv;

		public static List<SynchronizeFromCsv> Find(MongoConnection connection)
		{
			List<DatabaseSynchronizeFromCsv> databaseSynchronizeFromCsvList = DatabaseOptionBase.ReadAllowed<DatabaseSynchronizeFromCsv>(connection);

			return databaseSynchronizeFromCsvList.Select(databaseSynchronizeFromCsv => new SynchronizeFromCsv(connection, databaseSynchronizeFromCsv)).ToList();
		}

		protected override bool ExecuteOption()
		{
			string fileName = _databaseSynchronizeFromCsv.fileName;
			string fileNameTmp = _databaseSynchronizeFromCsv.fileNameTmp;
			char delimeter = _databaseSynchronizeFromCsv.delimeter;
			string keyName = _databaseSynchronizeFromCsv.keyName;
			string dateName = _databaseSynchronizeFromCsv.dateName;
			string[] fields = _databaseSynchronizeFromCsv.fields;
			Guid changeProviderId = _databaseSynchronizeFromCsv.changeProviderId;

			if
			(
				fields.Contains(keyName) == false ||
				fields.Contains(dateName) == false
			)
			{
				return false;
			}

			DateTime LatestModifiedDateTime = ContactChange.GetLatestModifiedOn(SqlConnection, changeProviderId);

			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			List<Dictionary<string, string>> csvData = csv.ReadLatest(dateName, LatestModifiedDateTime);

			DataLayer.SqlData.ChangeProvider changeProvider = DataLayer.SqlData.ChangeProvider.Read(SqlConnection, changeProviderId);

			ProcessCsvData(changeProviderId, csvData, dateName);

			return true;
		}

		private void ProcessCsvData(Guid changeProviderId, List<Dictionary<string, string>> csvData, string dateName)
		{
			foreach (Dictionary<string, string> csvRow in csvData)
			{
				Guid externalContactId = GetIdFromRow(csvRow);

				Contact contact = ReadOrCreateContact(csvRow, externalContactId, dateName);

				Guid contactId = contact.Id;

				ExternalContact externalContact = ExternalContact.ReadOrCreate(SqlConnection, externalContactId, changeProviderId, contactId);

				DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName]);

				bool ContactChangeExists = ContactChange.ContactChangeExists(SqlConnection, contactId, externalContactId, changeProviderId, collectedDate);

				if (ContactChangeExists == true)
				{
					continue;
				}

				CreateContactChange(changeProviderId, csvRow, externalContactId, contactId, collectedDate);
			}
		}

		private Contact ReadOrCreateContact(Dictionary<string, string> csvRow, Guid externalContactId, string dateName)
		{
			Contact contact = ContactCsvMapping.FindContact(SqlConnection, externalContactId, csvRow["firstName"]);

			if (contact == null)
			{
				contact = CreateContact(SqlConnection, csvRow, dateName);
			}

			return contact;
		}

		private void CreateContactChange(Guid changeProviderId, Dictionary<string, string> csvRow, Guid externalContactId, Guid contactId, DateTime collectedDate)
		{
			ContactChange contactChange = new ContactChange(SqlConnection, contactId, externalContactId, changeProviderId);

			contactChange.createdon = collectedDate;
			contactChange.modifiedon = collectedDate;

			foreach (string key in csvRow.Keys)
			{
				Utilities.ReflectionHelper.SetValue(contactChange, key, csvRow[key]);
			}

			contactChange.Insert();
		}

		private Contact CreateContact(SqlConnection sqlConnection, Dictionary<string, string> csvRow, string dateName)
		{
			DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName]);

			Contact contact = new Contact()
			{
				createdon = DateTime.Now,
				modifiedon = collectedDate,
			};

			foreach(string key in csvRow.Keys)
			{
				Utilities.ReflectionHelper.SetValue(contact, key, csvRow[key]);
			}

			contact.Insert(SqlConnection);

			return contact;
		}

		private Guid GetIdFromRow(Dictionary<string, string> csvRow)
		{
			int idInt;
			int.TryParse(csvRow["id"], out idInt);
			Guid id = Utilities.Converter.GuidConverter.Convert(0, 0, 0, idInt);

			return id;
		}
	}
}
