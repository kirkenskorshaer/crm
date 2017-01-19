using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Reflection;
using Utilities;
using MongoDB.Bson;

namespace DataLayer.MongoData.Option
{
	public abstract class OptionBase : AbstractMongoData
	{
		public Schedule Schedule { get; set; }
		public string Name { get; set; }

		protected static void Create<TOptionType>(MongoConnection connection, TOptionType option, string name, Schedule schedule)
		where TOptionType : OptionBase
		{
			option.Name = name;
			option.Schedule = schedule;

			Create(connection, option);
		}

		public static List<TOptionType> ReadAllowed<TOptionType>(MongoConnection connection, Worker worker)
		where TOptionType : OptionBase
		{
			ObjectId? workerId = worker?._id;

			Expression<Func<TOptionType, bool>> filter = option =>
				option.Schedule.NextAllowedExecution <= DateTime.Now &&
				option.Schedule.WorkerId == workerId &&
				option.Schedule.Enabled == true;

			IMongoCollection<TOptionType> options = connection.Database.GetCollection<TOptionType>(typeof(TOptionType).Name);
			IFindFluent<TOptionType, TOptionType> configFind = options.Find(filter);
			Task<List<TOptionType>> optionTask = configFind.ToListAsync();

			return optionTask.Result;
		}

		public void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				UpdateOption(connection);
			}
			else
			{
				DeleteOption(connection);
			}
		}

		public void Execute(MongoConnection connection)
		{
			if (Schedule.Recurring)
			{
				Schedule.MoveNext();
				Execute(connection, true);
			}
			else
			{
				Execute(connection, false);
			}
		}

		public void ExecuteFail(MongoConnection connection)
		{
			if (Schedule.Fails == null)
			{
				Schedule.Fails = 1;
			}
			else
			{
				Schedule.Fails++;
			}

			switch (Schedule.ActionOnFail)
			{
				case Schedule.ActionOnFailEnum.Disable:
					Schedule.Enabled = false;
					break;
				case Schedule.ActionOnFailEnum.TryAgain:
					Schedule.MoveNext();
					break;
				default:
					break;
			}

			UpdateOption(connection);
		}

		public void UpdateOption(MongoConnection connection)
		{
			MethodInfo method = typeof(AbstractMongoData).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

			MethodInfo generic = method.MakeGenericMethod(GetType());

			generic.Invoke(this, new object[] { connection });
		}

		public void DeleteOption(MongoConnection connection)
		{
			MethodInfo method = typeof(AbstractMongoData).GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance);

			MethodInfo generic = method.MakeGenericMethod(GetType());

			generic.Invoke(this, new object[] { connection });
		}

		public static void AssignWorkerToAllUnassigned<TOptionType>(Worker worker, MongoConnection mongoConnection)
		where TOptionType : OptionBase
		{
			IMongoCollection<OptionBase> datas = mongoConnection.Database.GetCollection<OptionBase>(typeof(TOptionType).Name);

			FilterDefinition<OptionBase> filter = Builders<OptionBase>.Filter.Eq(data => data.Schedule.WorkerId, null);

			UpdateDefinition<OptionBase> update = Builders<OptionBase>.Update.Set(option => option.Schedule.WorkerId, worker._id);

			Task<UpdateResult> result = datas.UpdateManyAsync(filter, update);

			MongoDataHelper.WaitForTaskOrThrowTimeout(result);
		}

		public static void UnAssignWorkerFromAllAssigned(Worker worker, MongoConnection mongoConnection)
		{
			List<Type> optionTypes = ReflectionHelper.GetChildTypes(typeof(OptionBase));

			foreach (Type optionType in optionTypes)
			{
				UnAssignWorkerFromAllAssigned(worker, mongoConnection, optionType.Name);
			}
		}

		private static void UnAssignWorkerFromAllAssigned(Worker worker, MongoConnection mongoConnection, string optionName)
		{
			IMongoCollection<OptionBase> datas = mongoConnection.Database.GetCollection<OptionBase>(optionName);

			FilterDefinition<OptionBase> filter = Builders<OptionBase>.Filter.Eq(data => data.Schedule.WorkerId, worker._id);

			UpdateDefinition<OptionBase> update = Builders<OptionBase>.Update.Set(option => option.Schedule.WorkerId, null);

			Task<UpdateResult> result = datas.UpdateManyAsync(filter, update);

			MongoDataHelper.WaitForTaskOrThrowTimeout(result);
		}

		public void AssignWorkerIfNoWorkerAssigned(Worker worker, MongoConnection mongoConnection)
		{
			IMongoCollection<OptionBase> datas = mongoConnection.Database.GetCollection<OptionBase>(GetType().Name);

			FilterDefinition<OptionBase> filter = Builders<OptionBase>.Filter.Eq(data => data.Schedule.WorkerId, null);
			filter = filter & Builders<OptionBase>.Filter.Eq(data => data._id, _id);

			UpdateDefinition<OptionBase> update = Builders<OptionBase>.Update.Set(option => option.Schedule.WorkerId, worker._id);

			Task<UpdateResult> result = datas.UpdateOneAsync(filter, update);

			MongoDataHelper.WaitForTaskOrThrowTimeout(result);
		}

		public List<string> GetUnassignedOptionNames(MongoConnection mongoConnection)
		{
			List<string> unassignedTypes = new List<string>();

			List<Type> optionTypes = ReflectionHelper.GetChildTypes(typeof(OptionBase));

			List<OptionBase> options = new List<OptionBase>();

			foreach (Type optionType in optionTypes)
			{
				bool hasUnassignedOptions = HasUnassignedOptions(mongoConnection, optionType);

				if (hasUnassignedOptions)
				{
					unassignedTypes.Add(optionType.Name);
				}
			}

			return unassignedTypes;
		}

		private static bool HasUnassignedOptions(MongoConnection mongoConnection, Type optionType)
		{
			IMongoCollection<OptionBase> datas = mongoConnection.Database.GetCollection<OptionBase>(optionType.Name);

			FilterDefinition<OptionBase> filter = Builders<OptionBase>.Filter.Eq(data => data.Schedule.WorkerId, null);

			ProjectionDefinition<OptionBase, ObjectId> projection = Builders<OptionBase>.Projection.Include(option => option._id);

			IFindFluent<OptionBase, ObjectId> find = datas.Find(filter).Project(projection);
			find.Limit(1);

			Task<ObjectId> dataTask = find.SingleOrDefaultAsync();

			return dataTask.Result != null;
		}
	}
}
