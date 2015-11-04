using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class UrlLogin
	{
		private static readonly string Name = "urlLogin";

		public ObjectId _id { get; set; }
		public string UrlName { get; set; }
		public string Url { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public static UrlLogin GetFirst(MongoConnection connection)
		{
			IMongoCollection<UrlLogin> urlLoginCollection = connection.Database.GetCollection<UrlLogin>(Name);
			IFindFluent<UrlLogin, UrlLogin> urlLoginFind = urlLoginCollection.Find(urlLogin => true);
			Task<UrlLogin> urlLoginTask = urlLoginFind.FirstAsync();

			return urlLoginTask.Result;
		}

		public static UrlLogin GetUrlLogin(MongoConnection connection, string urlName)
		{
			IMongoCollection<UrlLogin> urlLoginCollection = connection.Database.GetCollection<UrlLogin>(Name);
			IFindFluent<UrlLogin, UrlLogin> urlLoginFind = urlLoginCollection.Find(urlLogin => urlLogin.UrlName == urlName);
			Task<UrlLogin> urlLoginTask = urlLoginFind.SingleAsync();

			return urlLoginTask.Result;
		}

		public static bool Exists(MongoConnection connection, string urlName)
		{
			IMongoCollection<UrlLogin> urlLoginCollection = connection.Database.GetCollection<UrlLogin>(Name);
			Task<long> urlLoginTask = urlLoginCollection.CountAsync(urlLogin => urlLogin.UrlName == urlName);

			return urlLoginTask.Result > 0;
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<UrlLogin> urlLoginCollection = connection.Database.GetCollection<UrlLogin>(Name);
			Task insertTask = urlLoginCollection.InsertOneAsync(this);
			insertTask.Wait();
		}

		public void Delete(MongoConnection connection)
		{
			IMongoCollection<UrlLogin> urlLoginCollection = connection.Database.GetCollection<UrlLogin>(Name);
			Task deleteTask = urlLoginCollection.DeleteOneAsync(urlLogin => urlLogin._id == _id);
			deleteTask.Wait();
		}
	}
}
