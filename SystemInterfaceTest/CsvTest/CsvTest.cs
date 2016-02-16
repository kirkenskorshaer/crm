using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemInterface.Csv;

namespace SystemInterfaceTest.CsvTest
{
	[TestFixture]
	public class CsvTest
	{
		private string _fileName = @"C:\test\csv\test.csv";
		private string _fileNameTmp = @"C:\test\csv\test_tmp.csv";

		[SetUp]
		public void Setup()
		{
			if (File.Exists(_fileName))
			{
				File.Delete(_fileName);
			}
			if (File.Exists(_fileNameTmp))
			{
				File.Delete(_fileNameTmp);
			}
		}

		[Test]
		public void ConstructorFailsOnMissingFileAndNoColumns()
		{
			Assert.Throws(typeof(Exception), () => new Csv(';', _fileName, _fileNameTmp));
		}

		[Test]
		public void WriteLineWritesALine()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "id"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "name"));

			csv.WriteLine("1", "name1");

			string[] csvResult = File.ReadAllLines(_fileName);

			Assert.AreEqual("id;name", csvResult[0]);
			Assert.AreEqual("1;name1", csvResult[1]);
		}

		[Test]
		public void UpdateUpdates()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "id"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "name"));

			csv.WriteLine("1", "name1");
			csv.WriteLine("2", "name2");
			csv.WriteLine("3", "name3");

			csv.Update("id", "2", "2", "name2AfterUpdate");

			string[] csvResult = File.ReadAllLines(_fileName);

			Assert.AreEqual("id;name", csvResult[0]);
			Assert.AreEqual("1;name1", csvResult[1]);
			Assert.AreEqual("2;name2AfterUpdate", csvResult[2]);
			Assert.AreEqual("3;name3", csvResult[3]);
		}

		[Test]
		public void ReadReadsMultipleLines()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "id"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "name"));

			csv.WriteLine("1", "name1");
			csv.WriteLine("2", "name21");
			csv.WriteLine("2", "name22");
			csv.WriteLine("3", "name3");

			List<Dictionary<string, object>> recoveredValues = csv.ReadFields("id", "2");

			Assert.AreEqual("name21", recoveredValues[0]["name"]);
			Assert.AreEqual("name22", recoveredValues[1]["name"]);
		}

		[Test]
		public void DeleteRemovesLines()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "id"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "name"));

			csv.WriteLine("1", "name1");
			csv.WriteLine("2", "name21");
			csv.WriteLine("2", "name22");
			csv.WriteLine("3", "name3");

			csv.Delete("id", "2");

			string[] csvResult = File.ReadAllLines(_fileName);

			Assert.AreEqual("id;name", csvResult[0]);
			Assert.AreEqual("1;name1", csvResult[1]);
			Assert.AreEqual("3;name3", csvResult[2]);
			Assert.AreEqual(3, csvResult.Count());
		}

		[Test]
		public void ReadLatestReturnsOnlyLatestRows()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "id"), new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, "changeDate"));

			DateTime testDate = new DateTime(2000, 1, 13, 1, 2, 3);
			string dateFormat = "yyyyMMdd HH:mm:ss";

			csv.WriteLine("1", testDate.AddDays(1).ToString(dateFormat));
			csv.WriteLine("2", testDate.AddDays(2).ToString(dateFormat));
			csv.WriteLine("3", testDate.AddDays(3).ToString(dateFormat));
			csv.WriteLine("4", testDate.AddDays(4).ToString(dateFormat));

			List<Dictionary<string, object>> recoverdValues = csv.ReadLatest("id", "changeDate", testDate.AddDays(2));

			Assert.AreEqual(3, recoverdValues.Count);
			Assert.AreEqual(testDate.AddDays(2).ToString(dateFormat), recoverdValues[0]["changeDate"]);
			Assert.AreEqual(testDate.AddDays(3).ToString(dateFormat), recoverdValues[1]["changeDate"]);
			Assert.AreEqual(testDate.AddDays(4).ToString(dateFormat), recoverdValues[2]["changeDate"]);
		}
	}
}
