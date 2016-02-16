using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseSynchronizeFromCsv = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCsv;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SynchronizeFromCsvTest : TestBase
	{
		private DatabaseChangeProvider _changeProvider1;
		private DatabaseChangeProvider _changeProvider2;

		private string[] fields = new string[] { "id", "collectedDate", "firstName", "test" };
		private string fileName1 = @"C:\test\csv\test1.csv";
		private string fileName2 = @"C:\test\csv\test2.csv";
		private string fileNameTmp = @"C:\test\csv\test_tmp.csv";
		private char delimeter = ';';

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			string testCsvProvider1 = "testCsvProvider1";
			string testCsvProvider2 = "testCsvProvider2";

			_changeProvider1 = FindOrCreateChangeProvider(_sqlConnection, testCsvProvider1);
			_changeProvider2 = FindOrCreateChangeProvider(_sqlConnection, testCsvProvider2);

			if (File.Exists(fileName1))
			{
				File.Delete(fileName1);
			}
		}

		private DatabaseSynchronizeFromCsv GetDatabaseSynchronizeFromCsv(DatabaseChangeProvider changeProvider, string fileName)
		{
			return new DatabaseSynchronizeFromCsv()
			{
				changeProviderId = changeProvider.Id,
				dateName = "collectedDate",
				keyName = "id",
				delimeter = delimeter,
				fileName = fileName,
				fileNameTmp = fileNameTmp,
				Name = "importTestCsv",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				fields = fields,
			};
		}

		[Test]
		public void SynchronizeCreatesCorrectNumberOfContactChanges()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName1, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv(_changeProvider1, fileName1);
			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);

			Assert.AreEqual(2, databaseChanges.Count);
		}

		[Test]
		public void SynchronizeCreatesContactChangesWithCorrectData()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName1, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv(_changeProvider1, fileName1);
			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);

			DatabaseContactChange contactChange = databaseChanges.FirstOrDefault(change => change.firstname == "name1");

			Assert.NotNull(contactChange);
		}

		[Test]
		public void SynchronizeCreatesCorrectNumberOfContacts()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName1, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv(_changeProvider1, fileName1);

			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<DatabaseContact> contacts = databaseChanges.Select(contactChange => DatabaseContact.Read(_sqlConnection, contactChange.ContactId)).ToList();

			Assert.AreEqual(2, contacts.Count);
		}

		[Test]
		public void SynchronizeCreatesCorrectNumberOfExternalContacts()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName1, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv(_changeProvider1, fileName1);
			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<DatabaseExternalContact> externalContacts = databaseChanges.Select(contactChange => DatabaseExternalContact.Read(_sqlConnection, contactChange.ExternalContactId, _changeProvider1.Id)).ToList();

			Assert.AreEqual(2, externalContacts.Count);
		}

		[Test]
		public void Synchronize2CsvFilesDoesNotMakeExtraContacts()
		{
			SystemInterface.Csv.Csv csv1 = new SystemInterface.Csv.Csv(delimeter, fileName1, fileNameTmp, fields);

			csv1.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv1.WriteLine("2", "20010103 00:00:00", "name2", "234");

			SystemInterface.Csv.Csv csv2 = new SystemInterface.Csv.Csv(delimeter, fileName2, fileNameTmp, fields);

			csv2.WriteLine("2", "20010103 00:00:00", "name2", "234");
			csv2.WriteLine("3", "20010105 00:00:00", "name3", "345");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv1 = GetDatabaseSynchronizeFromCsv(_changeProvider1, fileName1);
			SynchronizeFromCsv synchronizeFromCsv1 = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv1);

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv2 = GetDatabaseSynchronizeFromCsv(_changeProvider2, fileName2);
			SynchronizeFromCsv synchronizeFromCsv2 = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv2);

			synchronizeFromCsv1.Execute();
			synchronizeFromCsv2.Execute();

			List<DatabaseContactChange> databaseChanges1 = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<DatabaseContactChange> databaseChanges2 = DatabaseContactChange.Read(_sqlConnection, _changeProvider2.Id, DatabaseContactChange.IdType.ChangeProviderId);

			List<DatabaseContact> contacts1 = databaseChanges1.Select(contactChange => DatabaseContact.Read(_sqlConnection, contactChange.ContactId)).ToList();
			List<DatabaseContact> contacts2 = databaseChanges2.Select(contactChange => DatabaseContact.Read(_sqlConnection, contactChange.ContactId)).ToList();

			Assert.AreEqual(3, contacts1.Union(contacts2).Count());
		}

		[Test]
		public void Synchronize2CsvFilesMakesExtraContactChanges()
		{
			SystemInterface.Csv.Csv csv1 = new SystemInterface.Csv.Csv(delimeter, fileName1, fileNameTmp, fields);

			csv1.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv1.WriteLine("2", "20010103 00:00:00", "name2", "234");

			SystemInterface.Csv.Csv csv2 = new SystemInterface.Csv.Csv(delimeter, fileName2, fileNameTmp, fields);

			csv2.WriteLine("2", "20010103 00:00:00", "name2", "234");
			csv2.WriteLine("3", "20010105 00:00:00", "name3", "345");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv1 = GetDatabaseSynchronizeFromCsv(_changeProvider1, fileName1);
			SynchronizeFromCsv synchronizeFromCsv1 = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv1);

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv2 = GetDatabaseSynchronizeFromCsv(_changeProvider2, fileName2);
			SynchronizeFromCsv synchronizeFromCsv2 = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv2);

			synchronizeFromCsv1.Execute();
			synchronizeFromCsv2.Execute();

			List<DatabaseContactChange> databaseChanges1 = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<DatabaseContactChange> databaseChanges2 = DatabaseContactChange.Read(_sqlConnection, _changeProvider2.Id, DatabaseContactChange.IdType.ChangeProviderId);

			Assert.AreEqual(4, databaseChanges1.Union(databaseChanges2).Count());
		}
	}
}
