using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

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

		public static List<TOptionType> ReadAllowed<TOptionType>(MongoConnection connection)
		where TOptionType : OptionBase
		{
			Expression<Func<TOptionType, bool>> filter = option => option.Schedule.NextAllowedExecution <= DateTime.Now;

			IMongoCollection<TOptionType> options = connection.Database.GetCollection<TOptionType>(typeof(TOptionType).Name);
			IFindFluent<TOptionType, TOptionType> configFind = options.Find(filter);
			Task<List<TOptionType>> optionTask = configFind.ToListAsync();

			return optionTask.Result;
		}

		protected abstract void Execute(MongoConnection connection, bool recurring);

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
	}
}
