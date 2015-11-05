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
		private SqlConnection _sqlConnection;

		private string[] fields = new string[] { "id", "collectedDate", "firstName", "test" };
		private string fileName = @"C:\test\csv\test.csv";
		private string fileNameTmp = @"C:\test\csv\test_tmp.csv";
		private char delimeter = ';';

		[SetUp]
		public void SetUp()
		{
			_sqlConnection = DataLayer.SqlConnectionHolder.GetConnection(Connection, "sql");

			List<DatabaseChangeProvider> changeProviders = DatabaseChangeProvider.ReadAll(_sqlConnection);

			string testCsvProvider1 = "testCsvProvider1";
			string testCsvProvider2 = "testCsvProvider2";

			_changeProvider1 = FindOrCreateChangeProvider(testCsvProvider1, changeProviders);
			_changeProvider2 = FindOrCreateChangeProvider(testCsvProvider2, changeProviders);

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		[TearDown]
		public void TearDown()
		{
			_changeProvider1.Delete(_sqlConnection);
			_changeProvider2.Delete(_sqlConnection);
		}

		private DatabaseChangeProvider FindOrCreateChangeProvider(string testCsvProvider, List<DatabaseChangeProvider> changeProviders)
		{
			Func<DatabaseChangeProvider, bool> findChangeProvider = lChangeProvider => lChangeProvider.Name == testCsvProvider;

			if (changeProviders.Any(findChangeProvider))
			{
				return changeProviders.Single(findChangeProvider);
			}

			DatabaseChangeProvider changeProvider = new DatabaseChangeProvider()
			{
				Name = testCsvProvider,
			};

			changeProvider.Insert(_sqlConnection);

			return changeProvider;
		}

		private DatabaseSynchronizeFromCsv GetDatabaseSynchronizeFromCsv()
		{
			return new DatabaseSynchronizeFromCsv()
			{
				changeProviderId = _changeProvider1.Id,
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
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv();
			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);

			Assert.AreEqual(2, databaseChanges.Count);
		}

		[Test]
		public void SynchronizeCreatesCorrectNumberOfContacts()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv();

			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<DatabaseContact> contacts = databaseChanges.Select(contactChange => DatabaseContact.Read(_sqlConnection, contactChange.ContactId)).ToList();

			Assert.AreEqual(2, contacts.Count);
		}

		[Test]
		public void SynchronizeCreatesCorrectNumberOfExternalContacts()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = GetDatabaseSynchronizeFromCsv();

			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();

			List<DatabaseContactChange> databaseChanges = DatabaseContactChange.Read(_sqlConnection, _changeProvider1.Id, DatabaseContactChange.IdType.ChangeProviderId);
			List<DatabaseExternalContact> externalContacts = databaseChanges.Select(contactChange => DatabaseExternalContact.Read(_sqlConnection, contactChange.ExternalContactId, _changeProvider1.Id)).ToList();

			Assert.AreEqual(2, externalContacts.Count);
		}

	}
}
