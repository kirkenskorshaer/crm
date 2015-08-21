using System.Collections.Generic;
using MongoDB.Driver;

namespace DataLayer
{
	public class MongoConnection
	{
		private readonly IMongoClient _client;
		internal readonly IMongoDatabase Database;
		private readonly string _databaseName;

		private static readonly Dictionary<string, MongoConnection> Connections = new Dictionary<string, MongoConnection>();

		private MongoConnection(string databaseName)
		{
			_client = new MongoClient();
			_databaseName = databaseName;
			Database = _client.GetDatabase(databaseName);
		}

		public static MongoConnection GetConnection(string databaseName)
		{
			if (Connections.ContainsKey(databaseName))
			{
				return Connections[databaseName];
			}

			MongoConnection connection = new MongoConnection(databaseName);

			Connections.Add(databaseName, connection);

			return connection;
		}

		public void DropDatabase()
		{
			_client.DropDatabaseAsync(_databaseName);
		}
	}
}
