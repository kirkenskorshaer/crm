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
			_sqlColumnsInfo = SqlUtilities.GetSqlColumnsInfo(GetType());
		}

		internal static void CreateIfMissing(SqlConnection sqlConnection, string tableName, List<string> columnsInDatabase, string name, SqlUtilities.DataType type, SqlBoolean allowNull)
		{
			if (columnsInDatabase.Contains(name) == false)
			{
				SqlUtilities.AddColumn(sqlConnection, tableName, name, type, allowNull);
			}
		}

		internal static void CreateKeyIfMissing(SqlConnection sqlConnection, string tableName, string foreignKeyName, string primaryTablename, string primaryKeyName, bool cascade = true)
		{
			SqlUtilities.MaintainForeignKey(sqlConnection, tableName, foreignKeyName, primaryTablename, primaryKeyName, cascade);
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
			return Read<ResultType>(sqlConnection, new SqlCondition(searchName, "=", searchValue), null, null);
		}

		internal static List<ResultType> Read<ResultType>(SqlConnection sqlConnection, SqlCondition sqlCondition) where ResultType : new()
		{
			return Read<ResultType>(sqlConnection, new List<SqlCondition> { sqlCondition }, null, null);
		}

		internal static List<ResultType> Read<ResultType>(SqlConnection sqlConnection, SqlCondition sqlCondition, int? top, string orderby) where ResultType : new()
		{
			return Read<ResultType>(sqlConnection, new List<SqlCondition> { sqlCondition }, top, orderby);
		}

		internal static List<ResultType> Read<ResultType>(SqlConnection sqlConnection, List<SqlCondition> sqlConditions) where ResultType : new()
		{
			return Read<ResultType>(sqlConnection, sqlConditions, null, null);
		}

		internal static List<ResultType> Read<ResultType>(SqlConnection sqlConnection, List<SqlCondition> sqlConditions, int? top, string orderby) where ResultType : new()
		{
			List<SqlColumnInfo> sqlColumnsInfo = SqlUtilities.GetSqlColumnsInfo(typeof(ResultType));

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
			if (top.HasValue)
			{
				sqlStringBuilder.AppendLine($"SELECT TOP {top.Value}");
			}
			else
			{
				sqlStringBuilder.AppendLine("SELECT");
			}
			sqlStringBuilder.Append(parameterStringBuilder);
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(ResultType).Name}]");
			sqlStringBuilder.AppendLine("WHERE");

			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			bool firstSqlConditionIteration = true;
			foreach (SqlCondition sqlCondition in sqlConditions)
			{
				if (firstSqlConditionIteration == false)
				{
					sqlStringBuilder.AppendLine("	AND");
				}
				sqlStringBuilder.AppendLine($"	{sqlCondition.searchName} {sqlCondition.operatorString} @{sqlCondition.searchName}");

				firstSqlConditionIteration = false;
				parameters.Add(new KeyValuePair<string, object>(sqlCondition.searchName, sqlCondition.searchValue));
			}

			if (string.IsNullOrWhiteSpace(orderby) == false)
			{
				sqlStringBuilder.AppendLine("ORDER BY");
				sqlStringBuilder.AppendLine($"	{orderby}");
			}

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());

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
					case SqlUtilities.DataType.NVARCHAR_MAX:
						value = ConvertFromDatabaseValue<string>(row[sqlColumnInfo.Name.ToLower()]);
						break;
					case SqlUtilities.DataType.INT:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<int?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<int>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					case SqlUtilities.DataType.DATETIME:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<DateTime?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<DateTime>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					case SqlUtilities.DataType.UNIQUEIDENTIFIER:
						if (sqlColumnInfo.SqlColumn.AllowNull)
						{
							value = ConvertFromDatabaseValue<Guid?>(row[sqlColumnInfo.Name.ToLower()]);
						}
						else
						{
							value = ConvertFromDatabaseValue<Guid>(row[sqlColumnInfo.Name.ToLower()]);
						}
						break;
					case SqlUtilities.DataType.BIT:
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

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(searchName, searchValue));

			return dataTable.Rows.Count > 0;
		}

		internal static ModelType ConvertFromDatabaseValue<ModelType>(object databaseObject)
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

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			foreach (SqlColumnInfo sqlColumn in _sqlColumnsInfo)
			{
				object value = NonSqlUtilities.ReflectionHelper.GetValue(this, sqlColumn.Name);
				AddInsertParameterIfNotNull(value, sqlColumn.Name.ToLower(), sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			}

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderColumns);
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderParameters);
			sqlStringBuilder.AppendLine(")");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());
		}
	}
}
