using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NonSqlUtilities = Utilities;

namespace DataLayer.SqlData
{
	public class AbstractIdData : AbstractData
	{
		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
		public Guid Id { get; protected set; }

		public void Delete(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("DELETE FROM");
			sqlStringBuilder.AppendLine($"	[{TableName}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, new KeyValuePair<string, object>("id", Id));
		}

		public new void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			List<SqlColumnInfo> dataColumns = _sqlColumnsInfo.Where(SqlColumnInfo.IsNotPrimaryKey).ToList();

			foreach (SqlColumnInfo sqlColumn in dataColumns)
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
			sqlStringBuilder.AppendLine("OUTPUT");
			sqlStringBuilder.AppendLine("	Inserted.id");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderParameters);
			sqlStringBuilder.AppendLine(")");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());

			DataRow row = dataTable.Rows[0];
			Id = (Guid)row["id"];
		}

		public void Update(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderSets = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			List<SqlColumnInfo> dataColumns = _sqlColumnsInfo.Where(SqlColumnInfo.IsNotPrimaryKey).ToList();

			foreach (SqlColumnInfo sqlColumn in dataColumns)
			{
				object value = NonSqlUtilities.ReflectionHelper.GetValue(this, sqlColumn.Name);
				AddUpdateParameter(value, sqlColumn.Name.ToLower(), sqlStringBuilderSets, parameters);
			}

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("Update");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("SET");
			sqlStringBuilder.Append(sqlStringBuilderSets);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			parameters.Add(new KeyValuePair<string, object>("id", Id));

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
		}

		public static ResultType ReadNextById<ResultType>(SqlConnection sqlConnection, Guid id) where ResultType : AbstractIdData, new()
		{
			List<ResultType> results = Read<ResultType>(sqlConnection, new SqlCondition("id", ">", id), 1, "id");

			if (results.Count == 1)
			{
				return results.Single();
			}

			if (id == Guid.Empty)
			{
				return null;
			}

			return ReadNextById<ResultType>(sqlConnection, Guid.Empty);
		}

		public override bool Equals(object obj)
		{
			if (GetType() != obj.GetType())
			{
				return false;
			}

			AbstractIdData abstractIdDataObj = obj as AbstractIdData;

			return Id == abstractIdDataObj.Id;
		}

		public override int GetHashCode()
		{
			string idAndType = Id.ToString() + GetType().GUID.ToString();
			return idAndType.GetHashCode();
		}

		protected static Guid? ReadIdFromField<DataType>(SqlConnection sqlConnection, string fieldName, string fieldValue) where DataType : AbstractIdData
		{
			List<SqlColumnInfo> sqlColumnsInfo = SqlUtilities.GetSqlColumnsInfo(typeof(DataType));

			if (sqlColumnsInfo.Any(sqlColumn => sqlColumn.Name == fieldName) == false)
			{
				throw new Exception($"Unknown fieldName {fieldName}");
			}

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(DataType).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{fieldName} = @{fieldName}");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(fieldName, fieldValue));

			if (dataTable.Rows.Count == 0)
			{
				return null;
			}

			DataRow row = dataTable.Rows[0];
			Guid id = AbstractData.ConvertFromDatabaseValue<Guid>(row["id"]);

			return id;
		}
	}
}
