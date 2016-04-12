using DataLayer;
using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options.Logic;
using NUnit.Framework;
using System;

namespace DataLayerTest.MongoDataTest.Option.Options.LogicTest
{
	[TestFixture]
	public class SynchronizeFromCsvTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();
		}

		[Test]
		public void Create()
		{
			Schedule schedule = new Schedule()
			{
				NextAllowedExecution = DateTime.Now,
				Recurring = false,
			};

			Guid changeProviderId = Guid.NewGuid();

			string filename = "filename";
			string filenameTmp = "filenameTmp";
			char delimeter = ',';
			string keyName = "BykoordinatorEmail";
			string dateName = "importDate";
			string mappingField = "emailaddress1";
			string[] fields = new string[]
			{
				"Firstname",
				"emailaddress1",
				"telephone1",
				"titel",
				"importDate"
			};

			SynchronizeFromCsv synchronizeFromCsv = SynchronizeFromCsv.Create(_connection, "test", schedule, changeProviderId, filename, filenameTmp, delimeter, keyName, dateName, mappingField, fields, null, SynchronizeFromCsv.ImportTypeEnum.Contact);
		}
	}
}
