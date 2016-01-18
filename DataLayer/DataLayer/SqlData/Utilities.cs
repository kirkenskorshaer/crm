using DataLayer.SqlData.Account;
using DataLayer.SqlData.Contact;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData
{
	public static class Utilities
	{
		public static DataTable ExecuteAdapterSelect(SqlConnection sqlConnection, StringBuilder sqlStringBuilder, params KeyValuePair<string, object>[] parameters)
		{
			SqlDataAdapter adapter = new SqlDataAdapter(sqlStringBuilder.ToString(), sqlConnection);

			for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
			{
				SqlParameter sqlParameter = new SqlParameter(parameters[parameterIndex].Key, parameters[parameterIndex].Value);
				adapter.SelectCommand.Parameters.Add(sqlParameter);
			}

			DataTable columnsTable = new DataTable();
			adapter.Fill(columnsTable);
			return columnsTable;
		}

		public static void ExecuteNonQuery(SqlConnection sqlConnection, StringBuilder sqlStringBuilder, CommandType commandType, params KeyValuePair<string, object>[] parameters)
		{
			SqlCommand sqlCommand = new SqlCommand(sqlStringBuilder.ToString(), sqlConnection);
			sqlCommand.CommandType = commandType;

			for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
			{
				object value = parameters[parameterIndex].Value;

				if(value == null)
				{
					value = DBNull.Value;
				}

				SqlParameter sqlParameter = new SqlParameter(parameters[parameterIndex].Key, value);

				sqlCommand.Parameters.Add(sqlParameter);
			}

			sqlConnection.Open();

			try
			{
				sqlCommand.ExecuteNonQuery();
			}
			finally
			{
				sqlConnection.Close();
			}
		}

		public static List<string> GetExistingColumns(SqlConnection sqlConnection, string tableName)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	COLUMNS.TABLE_NAME");
			sqlStringBuilder.AppendLine("	,COLUMNS.COLUMN_NAME");
			sqlStringBuilder.AppendLine("	,COLUMNS.DATA_TYPE");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	INFORMATION_SCHEMA.COLUMNS");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	COLUMNS.TABLE_NAME = @tableName");

			DataTable columnsTable = ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("tableName", tableName));

			List<string> columns = columnsTable.Select().Select(row => (string)row["COLUMN_NAME"]).ToList();

			return columns;
		}

		public static void CreateTable(SqlConnection sqlConnection, string tableName, string primaryKeyName)
		{
			Procedures.CreateTable.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateTable");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName", primaryKeyName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void CreateCompositeTable2Tables(SqlConnection sqlConnection, string tableName, string primaryKeyName1, string primaryKeyName2)
		{
			Procedures.CreateCompositeTable2Tables.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateCompositeTable2Tables");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName1", primaryKeyName1),
				new KeyValuePair<string, object>("primaryKeyName2", primaryKeyName2),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void CreateCompositeTable3Tables(SqlConnection sqlConnection, string tableName, string primaryKeyName1, string primaryKeyName2, string primaryKeyName3)
		{
			Procedures.CreateCompositeTable3Tables.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateCompositeTable3Tables");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName1", primaryKeyName1),
				new KeyValuePair<string, object>("primaryKeyName2", primaryKeyName2),
				new KeyValuePair<string, object>("primaryKeyName3", primaryKeyName3),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainAllTables(SqlConnection sqlConnection)
		{
			ChangeProvider.MaintainTable(sqlConnection);
			ExternalContact.MaintainTable(sqlConnection);
			Contact.Contact.MaintainTable(sqlConnection);
			ContactChange.MaintainTable(sqlConnection);
			ExternalAccount.MaintainTable(sqlConnection);
			Account.Account.MaintainTable(sqlConnection);
			AccountChange.MaintainTable(sqlConnection);
			AccountContact.MaintainTable(sqlConnection);
			AccountChangeContact.MaintainTable(sqlConnection);
			AccountIndsamler.MaintainTable(sqlConnection);
			AccountChangeIndsamler.MaintainTable(sqlConnection);
			Group.Group.MaintainTable(sqlConnection);
			Group.ContactGroup.MaintainTable(sqlConnection);
			Group.ContactChangeGroup.MaintainTable(sqlConnection);
			Group.AccountGroup.MaintainTable(sqlConnection);
			Group.AccountChangeGroup.MaintainTable(sqlConnection);
		}

		public static void DeleteAllTables(SqlConnection sqlConnection)
		{
			DropTable(sqlConnection, typeof(Group.ContactChangeGroup).Name);
			DropTable(sqlConnection, typeof(ContactChange).Name);
			DropTable(sqlConnection, typeof(Group.ContactGroup).Name);
			DropTable(sqlConnection, typeof(AccountContact).Name);
			DropTable(sqlConnection, typeof(AccountIndsamler).Name);
			DropTable(sqlConnection, typeof(AccountChangeIndsamler).Name);
			DropTable(sqlConnection, typeof(AccountChangeContact).Name);
			DropTable(sqlConnection, typeof(Contact.Contact).Name);
			DropTable(sqlConnection, typeof(ExternalContact).Name);

			DropTable(sqlConnection, typeof(Group.AccountChangeGroup).Name);
			DropTable(sqlConnection, typeof(AccountChange).Name);
			DropTable(sqlConnection, typeof(Group.AccountGroup).Name);
			DropTable(sqlConnection, typeof(Account.Account).Name);
			DropTable(sqlConnection, typeof(ExternalAccount).Name);

			DropTable(sqlConnection, typeof(Group.Group).Name);

			DropTable(sqlConnection, typeof(ChangeProvider).Name);
		}

		public static void RecreateAllTables(SqlConnection sqlConnection)
		{
			DeleteAllTables(sqlConnection);
			MaintainAllTables(sqlConnection);
		}

		public static void DropTable(SqlConnection sqlConnection, string tableName)
		{
			Procedures.DropTable.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("DropTable");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public enum DataType
		{
			NVARCHAR_MAX = 1
			, INT = 2
			, DATETIME = 3
			, UNIQUEIDENTIFIER = 4
			, BIT = 5
		}

		private static string GetTypeString(DataType type)
		{
			switch (type)
			{
				case DataType.NVARCHAR_MAX:
					return "NVARCHAR(MAX)";
				case DataType.INT:
					return "INT";
				case DataType.DATETIME:
					return "DATETIME";
				case DataType.UNIQUEIDENTIFIER:
					return "UNIQUEIDENTIFIER";
				case DataType.BIT:
					return "BIT";
				default:
					throw new ArgumentException($"unknown datatype {type}");
			}
		}

		public static void AddColumn(SqlConnection sqlConnection, string tableName, string columnName, DataType type, SqlBoolean allowNull)
		{
			Procedures.AddColumn.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("AddColumn");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("columnName", columnName),
				new KeyValuePair<string, object>("type", GetTypeString(type)),
				new KeyValuePair<string, object>("allowNull", allowNull),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainForeignKey(SqlConnection sqlConnection, string tableName, string foreignKeyName, string primaryTablename, string primaryKeyName)
		{
			Procedures.MaintainForeignKey.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("MaintainForeignKey");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("foreignKeyName", foreignKeyName),
				new KeyValuePair<string, object>("primaryTablename", primaryTablename),
				new KeyValuePair<string, object>("primaryKeyName", primaryKeyName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainCompositeForeignKey2Keys(SqlConnection sqlConnection, string tableName, string foreignKey1Name, string foreignKey2Name, string primaryTablename, string primaryKey1Name, string primaryKey2Name)
		{
			Procedures.MaintainCompositeForeignKey2Keys.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("MaintainCompositeForeignKey2Keys");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("foreignKey1Name", foreignKey1Name),
				new KeyValuePair<string, object>("foreignKey2Name", foreignKey2Name),
				new KeyValuePair<string, object>("primaryTablename", primaryTablename),
				new KeyValuePair<string, object>("primaryKey1Name", primaryKey1Name),
				new KeyValuePair<string, object>("primaryKey2Name", primaryKey2Name),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainUniqueConstraint(SqlConnection sqlConnection, string tableName, string constraintName, string constraintColumn1, string constraintColumn2)
		{
			Procedures.MaintainUniqueConstraint2Columns.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append(Procedures.MaintainUniqueConstraint2Columns.ProcedureName);

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("constraintName", constraintName),
				new KeyValuePair<string, object>("constraintColumn1", constraintColumn1),
				new KeyValuePair<string, object>("constraintColumn2", constraintColumn2),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainUniqueConstraint(SqlConnection sqlConnection, string tableName, string constraintName, string constraintColumn)
		{
			Procedures.MaintainUniqueConstraint1Column.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append(Procedures.MaintainUniqueConstraint1Column.ProcedureName);

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("constraintName", constraintName),
				new KeyValuePair<string, object>("constraintColumn", constraintColumn),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static DataType ReadNextById<DataType>(SqlConnection sqlConnection, Guid id, string[] columns, Func<DataRow, DataType> CreateFromRow)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT TOP 1");
			sqlStringBuilder.AppendLine("	id");
			foreach (string column in columns)
			{
				sqlStringBuilder.AppendLine($"	,{column}");
			}
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(DataType).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{typeof(DataType).Name}.id > @id");
			sqlStringBuilder.AppendLine("ORDER BY");
			sqlStringBuilder.AppendLine($"	{typeof(DataType).Name}.id");

			DataTable dataTable = ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			if (dataTable.Rows.Count == 1)
			{
				DataRow row = dataTable.Rows[0];

				DataType dataObject = CreateFromRow(row);

				return dataObject;
			}

			if (id == Guid.Empty)
			{
				return default(DataType);
			}

			return ReadNextById(sqlConnection, Guid.Empty, columns, CreateFromRow);
		}
	}
}
