using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Byarbejde;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
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

			List<string> columns = SqlUtilities.GetExistingColumns(_sqlConnection, tableName);

			Assert.AreEqual(0, columns.Count);
		}

		[Test]
		public void AddColumnAddsAColumn()
		{
			string tableName = GetRandomTableName();
			SqlUtilities.CreateTable(_sqlConnection, tableName, "id");
			List<string> columnsBefore = SqlUtilities.GetExistingColumns(_sqlConnection, tableName);

			SqlUtilities.AddColumn(_sqlConnection, tableName, "q", SqlUtilities.DataType.INT, SqlBoolean.False);

			List<string> columnsAfter = SqlUtilities.GetExistingColumns(_sqlConnection, tableName);
			SqlUtilities.DropTable(_sqlConnection, tableName);
			Assert.AreEqual(columnsBefore.Count + 1, columnsAfter.Count);
		}

		[Test]
		public void DropTableDeletesTable()
		{
			string tableName = GetRandomTableName();
			SqlUtilities.CreateTable(_sqlConnection, tableName, "id");
			List<string> columnsBefore = SqlUtilities.GetExistingColumns(_sqlConnection, tableName);

			SqlUtilities.DropTable(_sqlConnection, tableName);
			List<string> columnsAfter = SqlUtilities.GetExistingColumns(_sqlConnection, tableName);

			Assert.AreEqual(1, columnsBefore.Count);
			Assert.AreEqual(0, columnsAfter.Count);
		}

		[Test]
		public void CreateCompositeTableLinks2Tables()
		{
			string tableName1 = GetRandomTableName();
			string tableName2 = GetRandomTableName();
			string tableNameCompositeTable = GetRandomTableName();

			SqlUtilities.CreateTable(_sqlConnection, tableName1, "id");
			SqlUtilities.CreateTable(_sqlConnection, tableName2, "id");

			SqlUtilities.CreateCompositeTable2Tables(_sqlConnection, tableNameCompositeTable, "id1", "id2");

			SqlUtilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id1", tableName1, "id");
			SqlUtilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id2", tableName2, "id");

			Assert.Throws(typeof(SqlException), () => SqlUtilities.DropTable(_sqlConnection, tableName1));
			Assert.Throws(typeof(SqlException), () => SqlUtilities.DropTable(_sqlConnection, tableName2));

			SqlUtilities.DropTable(_sqlConnection, tableNameCompositeTable);
			SqlUtilities.DropTable(_sqlConnection, tableName1);
			SqlUtilities.DropTable(_sqlConnection, tableName2);
		}

		[Test]
		public void CreateCompositeTableLinks3Tables()
		{
			string tableName1 = GetRandomTableName();
			string tableName2 = GetRandomTableName();
			string tableName3 = GetRandomTableName();
			string tableNameCompositeTable = GetRandomTableName();

			SqlUtilities.CreateTable(_sqlConnection, tableName1, "id");
			SqlUtilities.CreateTable(_sqlConnection, tableName2, "id");
			SqlUtilities.CreateTable(_sqlConnection, tableName3, "id");

			SqlUtilities.CreateCompositeTable3Tables(_sqlConnection, tableNameCompositeTable, "id1", "id2", "id3");

			SqlUtilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id1", tableName1, "id");
			SqlUtilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id2", tableName2, "id");
			SqlUtilities.MaintainForeignKey(_sqlConnection, tableNameCompositeTable, "id3", tableName3, "id");

			Assert.Throws(typeof(SqlException), () => SqlUtilities.DropTable(_sqlConnection, tableName1));
			Assert.Throws(typeof(SqlException), () => SqlUtilities.DropTable(_sqlConnection, tableName2));
			Assert.Throws(typeof(SqlException), () => SqlUtilities.DropTable(_sqlConnection, tableName3));

			SqlUtilities.DropTable(_sqlConnection, tableNameCompositeTable);
			SqlUtilities.DropTable(_sqlConnection, tableName1);
			SqlUtilities.DropTable(_sqlConnection, tableName2);
			SqlUtilities.DropTable(_sqlConnection, tableName3);
		}

		[Test]
		public void MaintainCompositeForeignKey2KeysTest()
		{
			string tableName1 = GetRandomTableName();
			string tableName2 = GetRandomTableName();

			SqlUtilities.CreateCompositeTable2Tables(_sqlConnection, tableName1, "id1", "id2");

			SqlUtilities.CreateTable(_sqlConnection, tableName2, "id");
			SqlUtilities.AddColumn(_sqlConnection, tableName2, "id1", SqlUtilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			SqlUtilities.AddColumn(_sqlConnection, tableName2, "id2", SqlUtilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			SqlUtilities.MaintainCompositeForeignKey2Keys(_sqlConnection, tableName2, "id1", "id2", tableName1, "id1", "id2", true);

			Assert.Throws(typeof(SqlException), () => SqlUtilities.DropTable(_sqlConnection, tableName1));

			SqlUtilities.DropTable(_sqlConnection, tableName2);
			SqlUtilities.DropTable(_sqlConnection, tableName1);
		}

		[Test]
		public void MaintainUniqueConstraintTest()
		{
			string tableName = GetRandomTableName();
			string column1Name = GetRandomTableName();
			string column2Name = GetRandomTableName();

			SqlUtilities.CreateTable(_sqlConnection, tableName, "id");
			SqlUtilities.AddColumn(_sqlConnection, tableName, column1Name, SqlUtilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			SqlUtilities.AddColumn(_sqlConnection, tableName, column2Name, SqlUtilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			SqlUtilities.MaintainUniqueConstraint(_sqlConnection, tableName, "testConstraint", column1Name, column2Name);

			StringBuilder insertStringBuilder = InsertQuery(tableName, column1Name, column2Name);

			Guid column1Value = Guid.NewGuid();
			Guid column2Value = Guid.NewGuid();

			Action insertAction = () =>
			{
				SqlUtilities.ExecuteNonQuery(_sqlConnection, insertStringBuilder, System.Data.CommandType.Text
					, new KeyValuePair<string, object>("column1Value", column1Value)
					, new KeyValuePair<string, object>("column2Value", column2Value));
			};

			insertAction();

			Assert.Throws(typeof(SqlException), () => insertAction());

			SqlUtilities.DropTable(_sqlConnection, tableName);
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
		public void GetDataFieldsReturnsInheritedFields()
		{
			List<SqlColumnInfo> byArbejdeFields = SqlUtilities.GetSqlColumnsInfo(typeof(Byarbejde));

			Assert.IsTrue(byArbejdeFields.Any(byarbejde =>
				byarbejde.Name == "Id" &&
				byarbejde.SqlColumn.DataType == SqlUtilities.DataType.UNIQUEIDENTIFIER &&
				byarbejde.SqlColumn.AllowNull == false &&
				byarbejde.SqlColumn.Properties == SqlColumn.PropertyEnum.PrimaryKey));
		}

		[Test]
		public void GetDataFieldsReturnsInstanceFields()
		{
			List<SqlColumnInfo> byArbejdeFields = SqlUtilities.GetSqlColumnsInfo(typeof(Byarbejde));

			Assert.IsTrue(byArbejdeFields.Any(byarbejde =>
				byarbejde.Name == "Name" &&
				byarbejde.SqlColumn.DataType == SqlUtilities.DataType.NVARCHAR_MAX &&
				byarbejde.SqlColumn.AllowNull == false));
		}

		[Test]
		//[Ignore]
		public void MaintainTables()
		{
			SqlUtilities.DeleteAllTables(_sqlConnection);
			SqlUtilities.MaintainAllTables(_sqlConnection);
		}

		[Test]
		public void MainTainTablesCanCreateTablesLinkedByCompositeKeys()
		{
			SqlUtilities.MaintainTable(_sqlConnection, typeof(TableTest3Id));
			SqlUtilities.MaintainTable(_sqlConnection, typeof(TableTest2Id));
			SqlUtilities.MaintainTable(_sqlConnection, typeof(TableTest1Id));

			SqlUtilities.DropTable(_sqlConnection, typeof(TableTest1Id).Name);
			SqlUtilities.DropTable(_sqlConnection, typeof(TableTest2Id).Name);
			SqlUtilities.DropTable(_sqlConnection, typeof(TableTest3Id).Name);
		}

		private string GetRandomTableName()
		{
			return "T" + Guid.NewGuid().ToString().Replace('-', '_');
		}

		private class TableTest1Id : AbstractIdData
		{
			[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, new string[] { "FK1", "FK2" }, new Type[] { typeof(TableTest2Id), typeof(TableTest3Id) }, new string[] { "id1", "id1" }, new bool[] { true, false }, new int[] { 1, 1 })]
			public Guid? fkId1;
			[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, new string[] { "FK1", "FK2" }, new Type[] { typeof(TableTest2Id), typeof(TableTest3Id) }, new string[] { "id2", "id2" }, new bool[] { true, false }, new int[] { 1, 1 })]
			public Guid? fkId2;
			[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "FK2", typeof(TableTest3Id), "id3", false, 1)]
			public Guid? fkId3;
		}

		private class TableTest2Id : AbstractData
		{
			[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
			public Guid? id1;
			[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
			public Guid? id2;
		}

		private class TableTest3Id : AbstractData
		{
			[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
			public Guid? id1;
			[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
			public Guid? id2;
			[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
			public Guid? id3;
		}
	}
}
