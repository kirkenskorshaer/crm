using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using NonSqlUtilities = Utilities;

namespace DataLayer.SqlData
{
	public abstract class AbstractData
	{
		protected string TableName { get; set; }
		protected List<SqlColumnInfo> _sqlColumnsInfo;

		public AbstractData()
		{
			TableName = GetType().Name;
			_sqlColumnsInfo = Utilities.GetSqlColumnsInfo(GetType());
		}

		internal static void CreateIfMissing(SqlConnection sqlConnection, string tableName, List<string> columnsInDatabase, string name, Utilities.DataType type, SqlBoolean allowNull)
		{
			if (columnsInDatabase.Contains(name) == false)
			{
				Utilities.AddColumn(sqlConnection, tableName, name, type, allowNull);
			}
		}

		protected static void CreateKeyIfMissing(SqlConnection sqlConnection, string tableName, string foreignKeyName, string primaryTablename, string primaryKeyName, bool cascade = true)
		{
			Utilities.MaintainForeignKey(sqlConnection, tableName, foreignKeyName, primaryTablename, primaryKeyName, cascade);
		}

		protected void AddInsertParameterIfNotNull(object databaseObject, string databaseObjectName, StringBuilder sqlStringBuilderColumns, StringBuilder sqlStringBuilderParameters, List<KeyValuePair<string, object>> parameters)
		{
			if (databaseObject == null)
			{
				return;
			}

			if (databaseObject.GetType() == typeof(DateTime))
			{
				if (((DateTime)databaseObject) == DateTime.MinValue)
				{
					return;
				}
			}

			if (sqlStringBuilderColumns.Length == 0)
			{
				sqlStringBuilderColumns.Append("	");
				sqlStringBuilderParameters.Append("	");
			}
			else
			{
				sqlStringBuilderColumns.Append("	,");
				sqlStringBuilderParameters.Append("	,");
			}

			sqlStringBuilderColumns.AppendLine(databaseObjectName);
			sqlStringBuilderParameters.Append("@");
			sqlStringBuilderParameters.AppendLine(databaseObjectName);

			parameters.Add(new KeyValuePair<string, object>(databaseObjectName, databaseObject));
		}

		protected void AddUpdateParameter(object databaseObject, string databaseObjectName, StringBuilder sqlStringBuilderSets, List<KeyValuePair<string, object>> parameters)
		{
			if (sqlStringBuilderSets.Length == 0)
			{
				sqlStringBuilderSets.Append("	");
			}
			else
			{
				sqlStringBuilderSets.Append("	,");
			}

			sqlStringBuilderSets.Append(databaseObjectName);
			sqlStringBuilderSets.Append(" = @");
			sqlStringBuilderSets.AppendLine(databaseObjectName);

			parameters.Add(new KeyValuePair<string, object>(databaseObjectName, databaseObject));
		}

		internal static List<ResultType> Read<ResultType>(SqlConnection sqlConnection, string searchName, object searchValue) where ResultType : new()
		{
			List<SqlColumnInfo> sqlColumnsInfo = Utilities.GetSqlColumnsInfo(typeof(ResultType));

			StringBuilder parameterStringBuilder = new StringBuilder();
			foreach (SqlColumnInfo sqlColumn in sqlColumnsInfo)
			{
				if (parameterStringBuilder.Length == 0)
				{
					parameterStringBuilder.AppendLine($"	{sqlColumn.Name}");
				}
				else
				{
					parameterStringBuilder.AppendLine($"	,{sqlColumn.Name}");
				}
			}

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.Append(parameterStringBuilder);
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(ResultType).Name}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{searchName} = @{searchName}");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(searchName, searchValue));

			List<ResultType> results = new List<ResultType>();

			foreach (DataRow row in dataTable.Rows)
			{
				ResultType result = CreateFromRow<ResultType>(row, sqlColumnsInfo);

				results.Add(result);
			}

			return results;
		}

		private static ResultType CreateFromRow<ResultType>(DataRow row, List<SqlColumnInfo> sqlColumnsInfo) where ResultType : new()
		{
			ResultType result = new ResultType();

			foreach (SqlColumnInfo sqlColumnInfo in sqlColumnsInfo)
			{
				object value = null;

				switch (sqlColumnInfo.SqlColumn.DataType)
				{
					case Utilities.DataType.NVARCHAR_MAX:
						value = ConvertFromDatabaseValue<string>(row[sqlColumnInfo.Name.ToLower()]);
						break;
					case Utilities.DataType.INT:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<int?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<int>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					case Utilities.DataType.DATETIME:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<DateTime?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<DateTime>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					case Utilities.DataType.UNIQUEIDENTIFIER:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<Guid?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<Guid>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					case Utilities.DataType.BIT:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<bool?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<bool>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					default:
						break;
				}

				NonSqlUtilities.ReflectionHelper.SetValue(result, sqlColumnInfo.Name, value);
			}

			return result;
		}

		public static bool Exists<ResultType>(SqlConnection sqlConnection, string searchName, object searchValue)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT NULL WHERE EXISTS ( SELECT NULL FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(ResultType).Name}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{searchName} = @{searchName})");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(searchName, searchValue));

			return dataTable.Rows.Count > 0;
		}

		protected static ModelType ConvertFromDatabaseValue<ModelType>(object databaseObject)
		{
			if (databaseObject == null || databaseObject == DBNull.Value)
			{
				return default(ModelType);
			}
			else
			{
				return (ModelType)databaseObject;
			}
		}
	}
}
