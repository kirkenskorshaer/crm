using System;
using System.Collections.Generic;
using System.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq;

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
			string connectionString = ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
			_client = new MongoClient(connectionString);
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

		public void CleanDatabase()
		{
			List<string> CollectionsToPreserve = new List<string> { "system.indexes", "server", "sqlConnectionString", "urlLogin" };
			//List<string> CollectionsToPreserve = new List<string> { "system.indexes" };

			ListCollectionsOptions options = new ListCollectionsOptions()
			{
				Filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Nin("name", CollectionsToPreserve),
			};

			Task<IAsyncCursor<MongoDB.Bson.BsonDocument>> listTask = Database.ListCollectionsAsync(options);

			IAsyncCursor<MongoDB.Bson.BsonDocument> listCursor = listTask.Result;

			Action<MongoDB.Bson.BsonDocument> clearAction = (document) =>
			{
				string name = document.GetValue("name").AsString;

				IMongoCollection<MongoDB.Bson.BsonDocument> collection = Database.GetCollection<MongoDB.Bson.BsonDocument>(name);
				Task<DeleteResult> deleteTask = collection.DeleteManyAsync(lDocument => true);
				deleteTask.Wait();
			};

			Task clearTask = listCursor.ForEachAsync(clearAction);

			clearTask.Wait();
		}

		public List<Dictionary<string, object>> ReadAsDictionaries(string name)
		{
			IMongoCollection<MongoDB.Bson.BsonDocument> collection = Database.GetCollection<MongoDB.Bson.BsonDocument>(name);
			IFindFluent<MongoDB.Bson.BsonDocument, MongoDB.Bson.BsonDocument> findFluent = collection.Find(bson => true);

			Task<List<MongoDB.Bson.BsonDocument>> findTask = findFluent.ToListAsync();
			findTask.Wait();

			return findTask.Result.Select(bson => bson.ToDictionary()).ToList();
		}
	}
}
