using DataLayer;
using DataLayer.SqlData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;

namespace DataLayerTest.SqlDataTest
{
	[TestFixture]
	public class UtilitiesTest
	{
		private MongoConnection _mongoConnection;
		private SqlConnection _sqlConnection;

		[SetUp]
		public void SetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
		}

		[Test]
		public void GetExistingColumnsReturnsNoColumnsForUnknownTable()
		{
			string tableName = GetRandomTableName();

			List<string> columns = Utilities.GetExistingColumns(_sqlConnection, tableName);

			Assert.AreEqual(0, columns.Count);
		}

		[Test]
		public void AddColumnAddsAColumn()
		{
			string tableName = GetRandomTableName();
			Utilities.CreateTable(_sqlConnection, tableName, "id");
			List<string> columnsBefore = Utilities.GetExistingColumns(_sqlConnection, tableName);

			Utilities.AddColumn(_sqlConnection, tableName, "q", Utilities.DataType.INT, SqlBoolean.False);

			List<string> columnsAfter = Utilities.GetExistingColumns(_sqlConnection, tableName);
			Utilities.DropTable(_sqlConnection, tableName);
			Assert.AreEqual(columnsBefore.Count + 1, columnsAfter.Count);
		}

		[Test]
		public void DropTableDeletesTable()
		{
			string tableName = GetRandomTableName();
			Utilities.CreateTable(_sqlConnection, tableName, "id");
			List<string> columnsBefore = Utilities.GetExistingColumns(_sqlConnection, tableName);

			Utilities.DropTable(_sqlConnection, tableName);
			List<string> columnsAfter = Utilities.GetExistingColumns(_sqlConnection, tableName);

			Assert.AreEqual(1, columnsBefore.Count);
			Assert.AreEqual(0, columnsAfter.Count);
		}

		[Test]
		public void CreateCompositeTableLinks2Tables()
		{
			string tableName1 = GetRandomTableName();
			string tableName2 = GetRandomTableName();
			string tableNameCompositeTable = GetRandomTableName();

			Utilities.CreateTable(_sqlConnection, tableName1, "id");
			Utilities.CreateTable(_sqlConnection, tableName2, "id");

			Utilities.CreateCompositeTable2Tables(_sqlConnection, tableNameCompositeTable, "id1", "id2");

			Utilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id1", tableName1, "id");
			Utilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id2", tableName2, "id");

			Assert.Throws(typeof(SqlException), () => Utilities.DropTable(_sqlConnection, tableName1));
			Assert.Throws(typeof(SqlException), () => Utilities.DropTable(_sqlConnection, tableName2));

			Utilities.DropTable(_sqlConnection, tableNameCompositeTable);
			Utilities.DropTable(_sqlConnection, tableName1);
			Utilities.DropTable(_sqlConnection, tableName2);
		}

		[Test]
		public void CreateCompositeTableLinks3Tables()
		{
			string tableName1 = GetRandomTableName();
			string tableName2 = GetRandomTableName();
			string tableName3 = GetRandomTableName();
			string tableNameCompositeTable = GetRandomTableName();

			Utilities.CreateTable(_sqlConnection, tableName1, "id");
			Utilities.CreateTable(_sqlConnection, tableName2, "id");
			Utilities.CreateTable(_sqlConnection, tableName3, "id");

			Utilities.CreateCompositeTable3Tables(_sqlConnection, tableNameCompositeTable, "id1", "id2", "id3");

			Utilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id1", tableName1, "id");
			Utilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id2", tableName2, "id");
			Utilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id3", tableName3, "id");

			Assert.Throws(typeof(SqlException), () => Utilities.DropTable(_sqlConnection, tableName1));
			Assert.Throws(typeof(SqlException), () => Utilities.DropTable(_sqlConnection, tableName2));
			Assert.Throws(typeof(SqlException), () => Utilities.DropTable(_sqlConnection, tableName3));

			Utilities.DropTable(_sqlConnection, tableNameCompositeTable);
			Utilities.DropTable(_sqlConnection, tableName1);
			Utilities.DropTable(_sqlConnection, tableName2);
			Utilities.DropTable(_sqlConnection, tableName3);
		}

		[Test]
		public void MaintainCompositeForeignKey2KeysTest()
		{
			string tableName1 = GetRandomTableName();
			string tableName2 = GetRandomTableName();

			Utilities.CreateCompositeTable2Tables(_sqlConnection, tableName1, "id1", "id2");

			Utilities.CreateTable(_sqlConnection, tableName2, "id");
			Utilities.AddColumn(_sqlConnection, tableName2, "id1", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			Utilities.AddColumn(_sqlConnection, tableName2, "id2", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			Utilities.MaintainCompositeForeignKey2Keys(_sqlConnection, tableName2, "id1", "id2", tableName1, "id1", "id2");

			Assert.Throws(typeof(SqlException), () => Utilities.DropTable(_sqlConnection, tableName1));

			Utilities.DropTable(_sqlConnection, tableName2);
			Utilities.DropTable(_sqlConnection, tableName1);
		}

		[Test]
		public void MaintainUniqueConstraintTest()
		{
			string tableName = GetRandomTableName();
			string column1Name = GetRandomTableName();
			string column2Name = GetRandomTableName();

			Utilities.CreateTable(_sqlConnection, tableName, "id");
			Utilities.AddColumn(_sqlConnection, tableName, column1Name, Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			Utilities.AddColumn(_sqlConnection, tableName, column2Name, Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			Utilities.MaintainUniqueConstraint(_sqlConnection, tableName, "testConstraint", column1Name, column2Name);

			StringBuilder insertStringBuilder = InsertQuery(tableName, column1Name, column2Name);

			Guid column1Value = Guid.NewGuid();
			Guid column2Value = Guid.NewGuid();

			Action insertAction = () =>
			{
				Utilities.ExecuteNonQuery(_sqlConnection, insertStringBuilder, System.Data.CommandType.Text
					, new KeyValuePair<string, object>("column1Value", column1Value)
					, new KeyValuePair<string, object>("column2Value", column2Value));
			};

			insertAction();

			Assert.Throws(typeof(SqlException), () => insertAction());

			Utilities.DropTable(_sqlConnection, tableName);
		}

		private StringBuilder InsertQuery(string tableName, string column1Name, string column2Name)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + tableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	" + column1Name);
			sqlStringBuilder.AppendLine("	," + column2Name);
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("OUTPUT");
			sqlStringBuilder.AppendLine("	Inserted.id");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	@column1Value");
			sqlStringBuilder.AppendLine("	,@column2Value");
			sqlStringBuilder.AppendLine(")");

			return sqlStringBuilder;
		}

		[Test]
		[Ignore]
		public void MaintainTables()
		{
			Utilities.DeleteAllTables(_sqlConnection);
			Utilities.MaintainAllTables(_sqlConnection);
		}

		private string GetRandomTableName()
		{
			return "T" + Guid.NewGuid().ToString().Replace('-', '_');
		}
	}
}
