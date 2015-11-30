using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataLayer.MongoData
{
	public class Signal
	{
		public enum SignalTypeEnum
		{
			Fail = 1,
			Success = 2,
		};

		public ObjectId _id { get; set; }
		public string Option { get; set; }
		public double Strength { get; set; }
		public SignalTypeEnum SignalType { get; set; }

		public static List<Signal> ReadSignalsByOption(MongoConnection connection, string option)
		{
			IMongoCollection<Signal> collection = connection.Database.GetCollection<Signal>(typeof(Signal).Name);
			IFindFluent<Signal, Signal> signalFind = collection.Find(signal => signal.Option == option);
			Task<List<Signal>> signals = signalFind.ToListAsync();
			return signals.Result;
		}

		public static Signal ReadSignal(MongoConnection connection, string option, SignalTypeEnum signalType)
		{
			IMongoCollection<Signal> collection = connection.Database.GetCollection<Signal>(typeof(Signal).Name);
			IFindFluent<Signal, Signal> signalFind = collection.Find(lSignal =>
				lSignal.Option == option &&
				lSignal.SignalType == signalType);

			Task<Signal> signals = signalFind.SingleOrDefaultAsync();

			Signal signal = signals.Result;

			if (signal == null)
			{
				signal = new Signal()
				{
					Option = option,
					SignalType = SignalTypeEnum.Fail,
					Strength = 0d,
				};
			}

			return signal;
		}

		public static void IncreaseSignal(MongoConnection connection, string option, SignalTypeEnum signalType, double strength)
		{
			IMongoCollection<Signal> signalsCollection = connection.Database.GetCollection<Signal>(typeof(Signal).Name);

			FilterDefinition<Signal> filter =
				Builders<Signal>.Filter.Eq(signal => signal.Option, option) &
				Builders<Signal>.Filter.Eq(signal => signal.SignalType, signalType);

			long signalCount = signalsCollection.CountAsync(filter).Result;

			if (signalCount == 0)
			{
				Signal signal = new Signal()
				{
					Option = option,
					SignalType = signalType,
					Strength = strength,
				};

				Task insertTask = signalsCollection.InsertOneAsync(signal);

				insertTask.Wait();
			}
			else
			{
				UpdateDefinition<Signal> update = Builders<Signal>.Update.Inc(signal => signal.Strength, strength);

				Task updateTask = signalsCollection.UpdateOneAsync(filter, update);

				updateTask.Wait();
			}
		}

		public static void IncreaseSignal(MongoConnection connection, SignalTypeEnum signalType, double strength)
		{
			IMongoCollection<Signal> signalsCollection = connection.Database.GetCollection<Signal>(typeof(Signal).Name);

			FilterDefinition<Signal> filter = Builders<Signal>.Filter.Eq(signal => signal.SignalType, signalType);
			UpdateDefinition<Signal> update = Builders<Signal>.Update.Inc(signal => signal.Strength, strength);

			Task updateTask = signalsCollection.UpdateManyAsync(filter, update);

			updateTask.Wait();
		}

		public static void DecreaseAll(MongoConnection connection, double factor)
		{
			IMongoCollection<Signal> signalsCollection = connection.Database.GetCollection<Signal>(typeof(Signal).Name);

			UpdateDefinition<Signal> update = Builders<Signal>.Update.Mul(signal => signal.Strength, 1 / factor);

			Task updateTask = signalsCollection.UpdateManyAsync(signal => true, update);

			updateTask.Wait();
		}
	}
}
