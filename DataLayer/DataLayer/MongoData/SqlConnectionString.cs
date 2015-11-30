using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class SqlConnectionString
	{
		public ObjectId _id { get; set; }
		public string ConnectionName { get; set; }
		public string ConnectionString { get; set; }

		public static SqlConnectionString GetSqlConnectionString(MongoConnection connection, string connectionName)
		{
			IMongoCollection<SqlConnectionString> sqlConnectionStringCollection = connection.Database.GetCollection<SqlConnectionString>(typeof(SqlConnectionString).Name);
			IFindFluent<SqlConnectionString, SqlConnectionString> sqlConnectionStringFind = sqlConnectionStringCollection.Find(sqlConnectionString => sqlConnectionString.ConnectionName == connectionName);
			Task<SqlConnectionString> sqlConnectionStringTask = sqlConnectionStringFind.SingleAsync();

			return sqlConnectionStringTask.Result;
		}

		public static bool Exists(MongoConnection connection, string connectionName)
		{
			IMongoCollection<SqlConnectionString> sqlConnectionStringCollection = connection.Database.GetCollection<SqlConnectionString>(typeof(SqlConnectionString).Name);
			Task<long> sqlConnectionStringTask = sqlConnectionStringCollection.CountAsync(sqlConnectionString => sqlConnectionString.ConnectionName == connectionName);

			return sqlConnectionStringTask.Result > 0;
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<SqlConnectionString> sqlConnectionStringCollection = connection.Database.GetCollection<SqlConnectionString>(typeof(SqlConnectionString).Name);
			Task insertTask = sqlConnectionStringCollection.InsertOneAsync(this);
			insertTask.Wait();
		}

		public void Delete(MongoConnection connection)
		{
			IMongoCollection<SqlConnectionString> sqlConnectionStringCollection = connection.Database.GetCollection<SqlConnectionString>(typeof(SqlConnectionString).Name);
			Task deleteTask = sqlConnectionStringCollection.DeleteOneAsync(sqlConnectionString => sqlConnectionString._id == _id);
			deleteTask.Wait();
		}
	}
}
