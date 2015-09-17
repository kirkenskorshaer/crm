using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class Server
	{
		private static readonly string Name = "server";

		public ObjectId _id { get; set; }
		public string Ip { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public static Server GetFirst(MongoConnection connection)
		{
			IMongoCollection<Server> serverCollection = connection.Database.GetCollection<Server>(Name);
			IFindFluent<Server, Server> serverFind = serverCollection.Find(server => true);
			Task<Server> serverTask = serverFind.FirstAsync();

			return serverTask.Result;
		}

		public static Server GetServer(MongoConnection connection, string ip)
		{
			IMongoCollection<Server> serverCollection = connection.Database.GetCollection<Server>(Name);
			IFindFluent<Server, Server> serverFind = serverCollection.Find(server => server.Ip == ip);
			Task<Server> serverTask = serverFind.SingleAsync();

			return serverTask.Result;
		}

		public static bool Exists(MongoConnection connection, string ip)
		{
			IMongoCollection<Server> serverCollection = connection.Database.GetCollection<Server>(Name);
			Task<long> serverTask = serverCollection.CountAsync(server => server.Ip == ip);

			return serverTask.Result > 0;
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<Server> serverCollection = connection.Database.GetCollection<Server>(Name);
			Task insertTask = serverCollection.InsertOneAsync(this);
			insertTask.Wait();
		}
	}
}
