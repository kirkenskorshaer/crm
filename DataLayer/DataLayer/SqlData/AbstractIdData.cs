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
		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, Utilities.DataType.UNIQUEIDENTIFIER, false)]
		public Guid Id { get; protected set; }

		public void Delete(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("DELETE FROM");
			sqlStringBuilder.AppendLine($"	[{TableName}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, new KeyValuePair<string, object>("id", Id));
		}

		public void Insert(SqlConnection sqlConnection)
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

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());

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

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
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
	}
}
