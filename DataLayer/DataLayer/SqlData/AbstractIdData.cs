using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DataLayer.SqlData
{
	public class AbstractIdData : AbstractData
	{
		public Guid Id { get; protected set; }

		public void Delete(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("DELETE FROM");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, new KeyValuePair<string, object>("id", Id));
		}
	}
}
