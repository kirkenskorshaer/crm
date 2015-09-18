using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer.MongoData;

namespace DataLayer
{
	public class SqlConnectionHolder
	{
		private static readonly Dictionary<string, SqlConnection> Connections = new Dictionary<string, SqlConnection>();

		public static SqlConnection GetConnection(MongoConnection mongoConnection, string sqlConnectionName)
		{
			string connectionString = SqlConnectionString.GetSqlConnectionString(mongoConnection, sqlConnectionName).ConnectionString;

			if (Connections.ContainsKey(sqlConnectionName) == false)
			{
				SqlConnection connection = new SqlConnection(connectionString);
				Connections.Add(sqlConnectionName, connection);
			}

			return Connections[sqlConnectionName];
		}
	}
}
