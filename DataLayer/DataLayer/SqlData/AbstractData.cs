using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;

namespace DataLayer.SqlData
{
	public abstract class AbstractData
	{
		protected string TableName { get; set; }

		public AbstractData()
		{
			TableName = GetType().Name;
		}

		protected static void CreateIfMissing(SqlConnection sqlConnection, string tableName, List<string> columnsInDatabase, string name, Utilities.DataType type, SqlBoolean allowNull)
		{
			if (columnsInDatabase.Contains(name) == false)
			{
				Utilities.AddColumn(sqlConnection, tableName, name, type, allowNull);
			}
		}

		protected static void CreateKeyIfMissing(SqlConnection sqlConnection, string tableName, string foreignKeyName, string primaryTablename, string primaryKeyName)
		{
			Utilities.MaintainForeignKey(sqlConnection, tableName, foreignKeyName, primaryTablename, primaryKeyName);
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
