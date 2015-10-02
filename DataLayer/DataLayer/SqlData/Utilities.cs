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
				SqlParameter sqlParameter = new SqlParameter(parameters[parameterIndex].Key, parameters[parameterIndex].Value);
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
			Procedures.CreateTable.MakeSureCreateTableProcedureArePresent(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateTable");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName", primaryKeyName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void DropTable(SqlConnection sqlConnection, string tableName)
		{
			Procedures.DropTable.MakeSureDropTableProcedureArePresent(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("DropTable");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void AddColumn(SqlConnection sqlConnection, string tableName, string columnName, string type, SqlBoolean allowNull)
		{
			Procedures.AddColumn.MakeSureAddColumnProcedureArePresent(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("AddColumn");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("columnName", columnName),
				new KeyValuePair<string, object>("type", type),
				new KeyValuePair<string, object>("allowNull", allowNull),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainForeignKey(SqlConnection sqlConnection, string tableName, string foreignKeyName, string primaryTablename, string primaryKeyName)
		{
			Procedures.MaintainForeignKey.MakeSureMaintainForeignKeyProcedureArePresent(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("MaintainForeignKey");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("foreignKeyName", foreignKeyName),
				new KeyValuePair<string, object>("primaryTablename", primaryTablename),
				new KeyValuePair<string, object>("primaryKeyName", primaryKeyName),
				new KeyValuePair<string, object>("debug", 0));
		}
	}
}
