using System.Collections.Generic;
using System.Linq;
using DataLayer;
using DataLayer.MongoData;

namespace Administration.Option.Decider
{
	public class OptionDecider
	{
		private readonly MongoConnection _connection;

		public OptionDecider(MongoConnection connection)
		{
			_connection = connection;
		}

		public OptionBase Decide(List<OptionBase> options)
		{
			if (options.Any(option => option.GetType().Name == "Email"))
			{
				return options.First(option => option.GetType().Name == "Email");
			}

			return options.First();
		}
	}
}
