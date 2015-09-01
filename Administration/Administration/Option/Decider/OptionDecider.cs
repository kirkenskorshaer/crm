using System.Collections.Generic;
using System.Linq;

namespace Administration.Option.Decider
{
	public class OptionDecider
	{
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
