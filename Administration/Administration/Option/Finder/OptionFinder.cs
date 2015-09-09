using System.Collections.Generic;
using Administration.Option.Options;
using Administration.Option.Options.Service;
using DataLayer;

namespace Administration.Option.Finder
{
	public class OptionFinder
	{
		private readonly MongoConnection _connection;
		public OptionFinder(MongoConnection connection)
		{
			_connection = connection;
		}

		public List<OptionBase> Find()
		{
			List<OptionBase> options = new List<OptionBase>();
			options.AddRange(Email.Find(_connection));

			options.AddRange(ServiceCreate.Find(_connection));
			options.AddRange(ServiceDelete.Find(_connection));
			options.AddRange(ServiceStart.Find(_connection));
			options.AddRange(ServiceStop.Find(_connection));

			Sleep sleep = new Sleep(_connection);
			options.Add(sleep);

			return options;
		}
	}
}
