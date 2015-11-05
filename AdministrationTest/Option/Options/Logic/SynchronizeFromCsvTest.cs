using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseSynchronizeFromCsv = DataLayer.MongoData.Option.Options.Logic.SynchronizeFromCsv;
using DatabaseChangeProvider = DataLayer.SqlData.ChangeProvider;
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

		[Test]
		public void ExecuteOption()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = new DatabaseSynchronizeFromCsv()
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

			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv(Connection, databaseSynchronizeFromCsv);

			synchronizeFromCsv.Execute();
		}
	}
}
