using DataLayer;
using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options.Csv;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.MongoDataTest.Option.Options.CsvTest
{
	[TestFixture]
	public class CsvWriteLineTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.DropDatabase();
		}

		[Test]
		public void ReadTest()
		{
			CsvWriteLine csvWriteLine = CreateCsvWriteLine();

			List<CsvWriteLine> csvWriteLinesRead = CsvWriteLine.Read(_connection, csvWriteLine.Id);

			Assert.AreEqual(csvWriteLine.Id, csvWriteLinesRead.Single().Id);
			Assert.AreEqual(csvWriteLine.CsvElements[1].Value, csvWriteLinesRead.Single().CsvElements[1].Value);
		}

		private CsvWriteLine CreateCsvWriteLine()
		{
			Schedule schedule = MongoTestUtilities.CreateSchedule();
			string fileName = @"C:\test\csv\test.csv";
			string fileNameTmp = @"C:\test\csv\testTmp.csv";

			List<CsvElement> csvElements = new List<CsvElement>()
			{
				new CsvElement(){Key = "id", Value = "1",},
				new CsvElement(){Key = "name", Value = $"testname_{Guid.NewGuid()}",},
				new CsvElement(){Key = "changeDate", Value = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),},
			};

			CsvWriteLine csvWriteLine = CsvWriteLine.Create(_connection, "testLine", schedule, fileName, fileNameTmp, ';', csvElements);

			return csvWriteLine;
		}
	}
}
