using DataLayer;
using DataLayer.SqlData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace DataLayerTest.SqlData
{
	[TestFixture]
	public class UtilitiesTest
	{
		private MongoConnection _mongoConnection;

		[SetUp]
		public void SetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
		}

		[Test]
		public void GetExistingColumnsReturnsNoColumnsForUnknownTable()
		{
			SqlConnection sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "testMssql");
			string tableName = GetRandomTableName();

			List<string> columns = Utilities.GetExistingColumns(sqlConnection, tableName);

			Assert.AreEqual(0, columns.Count);
		}

		[Test]
		public void AddColumnAddsAColumn()
		{
			SqlConnection sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "testMssql");

			string tableName = GetRandomTableName();
			Utilities.CreateTable(sqlConnection, tableName, "id");
			List<string> columnsBefore = Utilities.GetExistingColumns(sqlConnection, tableName);

			Utilities.AddColumn(sqlConnection, tableName, "q", "INT", SqlBoolean.False);

			List<string> columnsAfter = Utilities.GetExistingColumns(sqlConnection, tableName);
			Utilities.DropTable(sqlConnection, tableName);
			Assert.AreEqual(columnsBefore.Count + 1, columnsAfter.Count);
		}

		[Test]
		public void DropTableDeletesTable()
		{
			SqlConnection sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "testMssql");

			string tableName = GetRandomTableName();
			Utilities.CreateTable(sqlConnection, tableName, "id");
			List<string> columnsBefore = Utilities.GetExistingColumns(sqlConnection, tableName);

			Utilities.DropTable(sqlConnection, tableName);
			List<string> columnsAfter = Utilities.GetExistingColumns(sqlConnection, tableName);

			Assert.AreEqual(1, columnsBefore.Count);
			Assert.AreEqual(0, columnsAfter.Count);
		}

		private string GetRandomTableName()
		{
			return "T" + Guid.NewGuid().ToString().Replace('-', '_');
		}
	}
}
