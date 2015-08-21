using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class Config
	{
		private static readonly string ConfigName = "config";

		public ObjectId _id { get; set; }

		public string Email { get; set; }
		public string EmailPassword { get; set; }
		public string EmailSmtpHost { get; set; }
		public int EmailSmtpPort { get; set; }

		public static Config GetConfig(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(ConfigName);
			IFindFluent<Config, Config> configFind = configs.Find(config => true);
			Task<Config> configTask = configFind.SingleAsync();

			return configTask.Result;
		}

		public static bool Exists(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(ConfigName);
			Task<long> configFind = configs.CountAsync(config => true);

			return configFind.Result > 0;
		}

		public void Update(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(ConfigName);

			FilterDefinition<Config> filter = Builders<Config>.Filter.Eq(config => config._id, _id);

			Task<ReplaceOneResult> result = configs.ReplaceOneAsync(filter, this);
			result.Wait();
		}

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(ConfigName);
			Task insertTask = configs.InsertOneAsync(this);
			insertTask.Wait();
		}
	}
}
