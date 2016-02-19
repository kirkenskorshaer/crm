using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseSynchronizeFromCsv = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCsv;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;
using DatabaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using DatabaseExternalContact = DataLayer.SqlData.Contact.ExternalContact;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using SystemInterface.Csv;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SynchronizeFromCsvTest : TestBase
	{
		private DatabaseChangeProvider _changeProvider1;
		private DatabaseChangeProvider _changeProvider2;

		private ColumnDefinition[] fields = new ColumnDefinition[] { new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "id"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "collectedDate"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "firstName"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "test") };
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
				fields = ColumnDefinition.ToDefinitionString(fields),
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

		[Test]
		[Ignore]
		public void ImportContacts()
		{
			DataLayer.MongoData.Option.Schedule schedule = new DataLayer.MongoData.Option.Schedule()
			{
				NextAllowedExecution = DateTime.Now,
				Recurring = false,
			};

			Guid changeProviderId = DatabaseChangeProvider.ReadByNameOrCreate(_sqlConnection, "csv-contacts").Id;

			string filename = "C:/Users/Svend/Documents/indsamlingssteder/contacts.csv";
			string filenameTmp = "C:/Users/Svend/Documents/indsamlingssteder/contacts.csv.tmp";
			char delimeter = '\t';
			string keyName = "id";
			string dateName = "importDate";
			string mappingField = "emailaddress1";
			string[] fields = new string[]
			{
				"id",
				"firstname",
				"lastname",
				"emailaddress1",
				"telephone1",
				"titel",
				"importDate",
				"comment",
				"bool:new_bykoordinator",
			};

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = DatabaseSynchronizeFromCsv.Create(Connection, "test", schedule, changeProviderId, filename, filenameTmp, delimeter, keyName, dateName, mappingField, fields, DatabaseSynchronizeFromCsv.ImportTypeEnum.Contact);

			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);
			synchronizeFromCsv.Execute();

			DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm databaseSynchronizeToCrm = new DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm()
			{
				changeProviderId = DatabaseChangeProvider.ReadByNameOrCreate(_sqlConnection, "crm").Id,
				Name = "test",
				Schedule = schedule,
				urlLoginName = "test",
				synchronizeType = DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm.SynchronizeTypeEnum.Contact,
			};

			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			MakeSureThereAreProgressOnAllContacts();

			for (int contactCount = 0; contactCount < 12; contactCount++)
			{
				synchronizeToCrm.Execute();
			}
		}

		[Test]
		[Ignore]
		public void ImportAccounts()
		{
			DataLayer.MongoData.Option.Schedule schedule = new DataLayer.MongoData.Option.Schedule()
			{
				NextAllowedExecution = DateTime.Now,
				Recurring = false,
			};

			Guid changeProviderId = DatabaseChangeProvider.ReadByNameOrCreate(_sqlConnection, "csv-accounts").Id;

			string filename = "C:/Users/Svend/Documents/indsamlingssteder/indsamlingssteder.csv";
			string filenameTmp = "C:/Users/Svend/Documents/indsamlingssteder/indsamlingssteder.csv.tmp";
			char delimeter = '\t';
			string keyName = "new_kkadminmedlemsnr";
			string dateName = "importDate";
			string mappingField = "emailaddress1";
			string[] fields = new string[]
			{
				"bool:new_erindsamlingssted",
				"int:new_kkadminmedlemsnr",
				"group",
				"name",
				"telephone1",
				"address1_line1",
				"address1_line2",
				"address1_postalcode",
				"address1_city",
				"BykoordinatorNavn",
				"bykoordinatoremail",
				"BykoordinatorTel",
				"BykoordinatorTitel",
				"comment",
				"SubRegion",
				"importDate",
				"kredsellerby",
				"error",
			};

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = DatabaseSynchronizeFromCsv.Create(Connection, "test", schedule, changeProviderId, filename, filenameTmp, delimeter, keyName, dateName, mappingField, fields, DatabaseSynchronizeFromCsv.ImportTypeEnum.Account);

			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);
			synchronizeFromCsv.Execute();

			DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm databaseSynchronizeToCrm = new DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm()
			{
				changeProviderId = DatabaseChangeProvider.ReadByNameOrCreate(_sqlConnection, "crm").Id,
				Name = "test",
				Schedule = schedule,
				urlLoginName = "test",
				synchronizeType = DataLayer.MongoData.Option.Options.Logic.SynchronizeToCrm.SynchronizeTypeEnum.Account,
			};

			SynchronizeToCrm synchronizeToCrm = new SynchronizeToCrm(Connection, databaseSynchronizeToCrm);

			MakeSureThereAreProgressOnAllAccounts();

			for (int contactCount = 0; contactCount < 283; contactCount++)
			//for (int contactCount = 0; contactCount < 10; contactCount++)
			{
				synchronizeToCrm.Execute();
			}
		}

		private void MakeSureThereAreProgressOnAllContacts()
		{
			Guid contactIdCurrent = DatabaseContact.ReadNextById(_sqlConnection, Guid.Empty).Id;
			Guid contactIdFirst = contactIdCurrent;
			Guid contactIdLast = Guid.Empty;
			DataLayer.MongoData.Progress progress = null;

			while (contactIdLast != contactIdFirst)
			{
				bool progressExists = DataLayer.MongoData.Progress.Exists(Connection, MaintainProgress.ProgressContactToCrm, contactIdCurrent);
				if (progressExists == false)
				{
					progress = new DataLayer.MongoData.Progress()
					{
						LastProgressDate = DateTime.Now,
						TargetName = MaintainProgress.ProgressContactToCrm,
						TargetId = contactIdCurrent,
					};
					progress.Insert(Connection);
				}

				contactIdCurrent = DatabaseContact.ReadNextById(_sqlConnection, contactIdCurrent).Id;
				contactIdLast = contactIdCurrent;
			}
		}

		private void MakeSureThereAreProgressOnAllAccounts()
		{
			Guid accountIdCurrent = DatabaseAccount.ReadNextById(_sqlConnection, Guid.Empty).Id;
			Guid accountIdFirst = accountIdCurrent;
			Guid accountIdLast = Guid.Empty;
			DataLayer.MongoData.Progress progress = null;

			while (accountIdLast != accountIdFirst)
			{
				bool progressExists = DataLayer.MongoData.Progress.Exists(Connection, MaintainProgress.ProgressAccountToCrm, accountIdCurrent);
				if (progressExists == false)
				{
					progress = new DataLayer.MongoData.Progress()
					{
						LastProgressDate = DateTime.Now,
						TargetName = MaintainProgress.ProgressAccountToCrm,
						TargetId = accountIdCurrent,
					};
					progress.Insert(Connection);
				}

				accountIdCurrent = DatabaseAccount.ReadNextById(_sqlConnection, accountIdCurrent).Id;
				accountIdLast = accountIdCurrent;
			}
		}
	}
}
