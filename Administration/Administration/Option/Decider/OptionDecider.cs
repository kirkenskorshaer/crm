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

			options = options.OrderByDescending(option => SignalStrength(option)).ToList();

			return options.First();
		}

		private double SignalStrength(OptionBase option)
		{
			string name = option.GetType().Name;

			if (name == "Sleep")
			{
				return 0d;
			}

			Signal failSignal = Signal.ReadSignal(_connection, option.DatabaseOption.GetType().Name, Signal.SignalTypeEnum.Fail);
			Signal successSignal = Signal.ReadSignal(_connection, option.DatabaseOption.GetType().Name, Signal.SignalTypeEnum.Success);

			if (failSignal.Strength >= 1)
			{
				return -1;
			}

			return 1-successSignal.Strength;
		}
	}
}
