using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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
