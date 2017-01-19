using System.Collections.Generic;
using DataLayer;
using Utilities;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseOptionFinder = DataLayer.MongoData.Option.Finder.OptionFinder;
using System;
using System.Linq;

namespace Administration.Option.Finder
{
	public class OptionFinder
	{
		private readonly MongoConnection _connection;
		private readonly DatabaseOptionFinder _databaseOptionFinder;

		public OptionFinder(MongoConnection connection)
		{
			_connection = connection;
			_databaseOptionFinder = new DatabaseOptionFinder(_connection);
		}

		public List<OptionBase> Find()
		{
			List<OptionBase> options = new List<OptionBase>();

			List<DatabaseOptionBase> databaseOptions = _databaseOptionFinder.Find();

			List<Type> optionTypes = ReflectionHelper.GetChildTypes(typeof(OptionBase));

			foreach (DatabaseOptionBase databaseOption in databaseOptions)
			{
				string optionName = databaseOption.GetType().Name;

				Type optionType = optionTypes.SingleOrDefault(type => type.Name == optionName);

				if (optionType == null)
				{
					Log.WriteLocation(_connection, $"Unknown option type {optionType}", "OptionFinder", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
					continue;
				}

				OptionBase option = (OptionBase)Activator.CreateInstance(optionType, _connection, databaseOption);

				options.Add(option);
			}

			return options;
		}
	}
}