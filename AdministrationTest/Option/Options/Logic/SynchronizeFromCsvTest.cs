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
		private DatabaseChangeProvider _changeProvider;
		private SqlConnection _sqlConnection;

		private string[] fields = new string[] { "id", "collectedDate", "firstName", "test" };
		private string fileName = @"C:\test\csv\test.csv";
		private string fileNameTmp = @"C:\test\csv\test_tmp.csv";
		private char delimeter = ';';

		[SetUp]
		public void SetUp()
		{
			string testCsvProvider = "testCsvProvider";

			_sqlConnection = DataLayer.SqlConnectionHolder.GetConnection(Connection, "sql");

			List<DatabaseChangeProvider> changeProviders = DatabaseChangeProvider.ReadAll(_sqlConnection);

			Func<DatabaseChangeProvider, bool> findChangeProvider = changeProvider => changeProvider.Name == testCsvProvider;

			if (changeProviders.Any(findChangeProvider))
			{
				_changeProvider = changeProviders.Single(findChangeProvider);
			}
			else
			{
				_changeProvider = new DatabaseChangeProvider()
				{
					Name = testCsvProvider,
				};

				_changeProvider.Insert(_sqlConnection);
			}

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		[Test]
		public void ExecuteOption()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(delimeter, fileName, fileNameTmp, fields);

			csv.WriteLine("1", "20010101 00:00:00", "name1", "123");
			csv.WriteLine("2", "20010103 00:00:00", "name2", "234");

			DatabaseSynchronizeFromCsv databaseSynchronizeFromCsv = new DatabaseSynchronizeFromCsv()
			{
				changeProviderId = _changeProvider.Id,
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
