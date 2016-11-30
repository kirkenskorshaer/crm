using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLayer.MongoData
{
	[BsonIgnoreExtraElements]
	public class Config
	{
		public ObjectId _id { get; set; }

		public string Email { get; set; }
		public string EmailPassword { get; set; }
		public string EmailSmtpHost { get; set; }
		public int EmailSmtpPort { get; set; }
		public LogLevelEnum LogLevel { get; set; }
		public string ResourcePath { get; set; }
		public string MailrelayUrl { get; set; }
		public string MailrelayApiKey { get; set; }
		public double MailrelaySendIntervalMilliseconds { get; set; }
		public bool? EnableTest { get; set; }
		public string DynamicsCrmUsername { get; set; }
		public string DynamicsCrmPassword { get; set; }
		public string DynamicsCrmUrl { get; set; }

		[Flags]
		public enum LogLevelEnum
		{
			HeartError = 1,
			HeartMessage = 2,
			OptionError = 4,
			OptionMessage = 8,
			OptionReport = 16,
		}

		public static Config GetConfig(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(typeof(Config).Name);
			IFindFluent<Config, Config> configFind = configs.Find(config => true);
			Task<Config> configTask = configFind.SingleAsync();

			return MongoDataHelper.GetValueOrThrowTimeout(configTask);
		}

		public static bool Exists(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(typeof(Config).Name);
			Task<long> configFind = configs.CountAsync(config => true);

			return configFind.Result > 0;
		}

		public void Update(MongoConnection connection)
		{
			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(typeof(Config).Name);

			FilterDefinition<Config> filter = Builders<Config>.Filter.Eq(config => config._id, _id);

			Task<ReplaceOneResult> result = configs.ReplaceOneAsync(filter, this);
			result.Wait();
		}

		public void Insert(MongoConnection connection)
		{
			if (Exists(connection))
			{
				throw new Exception("Config already exists");
			}

			IMongoCollection<Config> configs = connection.Database.GetCollection<Config>(typeof(Config).Name);
			Task insertTask = configs.InsertOneAsync(this);
			insertTask.Wait();
		}

		public string GetResourcePath(string path)
		{
			if (string.IsNullOrWhiteSpace(ResourcePath) == false)
			{
				return ResourcePath + "/" + path;
			}

			Assembly entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly != null)
			{
				return entryAssembly.Location + "/" + path;
			}

			return Directory.GetCurrentDirectory() + "/" + path;
		}

		public string GetOrCreateResourcePath(string path)
		{
			string fullPath = GetResourcePath(path);

			if (Directory.Exists(fullPath) == false)
			{
				Directory.CreateDirectory(fullPath);
			}

			return fullPath;
		}
	}
}
