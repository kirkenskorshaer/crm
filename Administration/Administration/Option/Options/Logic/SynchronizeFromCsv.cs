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
			SystemInterface.Csv.ColumnDefinition[] fields = SystemInterface.Csv.ColumnDefinition.Read(_databaseSynchronizeFromCsv.fields);
			Guid changeProviderId = _databaseSynchronizeFromCsv.changeProviderId;

			if
			(
				fields.Any(definition => definition.Name == keyName) == false ||
				fields.Any(definition => definition.Name == dateName) == false
			)
			{
				return false;
			}

			DateTime LatestModifiedDateTime = ContactChange.GetLatestModifiedOn(SqlConnection, changeProviderId);

			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			List<Dictionary<string, object>> csvData = csv.ReadLatest(keyName, dateName, LatestModifiedDateTime);

			DataLayer.SqlData.ChangeProvider changeProvider = DataLayer.SqlData.ChangeProvider.Read(SqlConnection, changeProviderId);

			ProcessCsvData(changeProviderId, csvData, dateName);

			return true;
		}

		private void ProcessCsvData(Guid changeProviderId, List<Dictionary<string, object>> csvData, string dateName)
		{
			foreach (Dictionary<string, object> csvRow in csvData)
			{
				Guid externalContactId = GetIdFromRow(csvRow);

				Contact contact = ReadOrCreateContact(csvRow, externalContactId, changeProviderId, dateName);

				Guid contactId = contact.Id;

				DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName].ToString());

				bool ContactChangeExists = ContactChange.ContactChangeExists(SqlConnection, contactId, externalContactId, changeProviderId, collectedDate);

				if (ContactChangeExists == true)
				{
					continue;
				}

				CreateContactChange(changeProviderId, csvRow, externalContactId, contactId, collectedDate);
			}
		}

		private Contact ReadOrCreateContact(Dictionary<string, object> csvRow, Guid externalContactId, Guid changeProviderId, string dateName)
		{
			bool externalContactExists = ExternalContact.Exists(SqlConnection, externalContactId, changeProviderId);

			ExternalContact externalContact = null;
			Contact contact = null;

			if (externalContactExists)
			{
				externalContact = ExternalContact.Read(SqlConnection, externalContactId, changeProviderId);
				contact = Contact.Read(SqlConnection, externalContact.ContactId);
			}
			else
			{
				contact = CreateContact(SqlConnection, csvRow, dateName);
				externalContact = new ExternalContact(SqlConnection, externalContactId, changeProviderId, contact.Id);
				externalContact.Insert();
			}

			return contact;
		}

		private void CreateContactChange(Guid changeProviderId, Dictionary<string, object> csvRow, Guid externalContactId, Guid contactId, DateTime collectedDate)
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

		private Contact CreateContact(SqlConnection sqlConnection, Dictionary<string, object> csvRow, string dateName)
		{
			DateTime collectedDate = Utilities.Converter.DateTimeConverter.DateTimeFromString(csvRow[dateName].ToString());

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

		private Guid GetIdFromRow(Dictionary<string, object> csvRow)
		{
			int idInt;
			int.TryParse(csvRow["id"].ToString(), out idInt);
			Guid id = Utilities.Converter.GuidConverter.Convert(0, 0, 0, idInt);

			return id;
		}
	}
}
