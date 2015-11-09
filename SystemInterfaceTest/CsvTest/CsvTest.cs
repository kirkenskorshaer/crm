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
			Csv csv = new Csv(';', _fileName, _fileNameTmp, "id", "name");

			csv.WriteLine("1", "name1");

			string[] csvResult = File.ReadAllLines(_fileName);

			Assert.AreEqual("id;name", csvResult[0]);
			Assert.AreEqual("1;name1", csvResult[1]);
		}

		[Test]
		public void UpdateUpdates()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, "id", "name");

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
			Csv csv = new Csv(';', _fileName, _fileNameTmp, "id", "name");

			csv.WriteLine("1", "name1");
			csv.WriteLine("2", "name21");
			csv.WriteLine("2", "name22");
			csv.WriteLine("3", "name3");

			List<Dictionary<string, string>> recoveredValues = csv.ReadFields("id", "2");

			Assert.AreEqual("name21", recoveredValues[0]["name"]);
			Assert.AreEqual("name22", recoveredValues[1]["name"]);
		}

		[Test]
		public void DeleteRemovesLines()
		{
			Csv csv = new Csv(';', _fileName, _fileNameTmp, "id", "name");

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
	}
}
