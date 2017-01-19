using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Administration.Option.Status;
using System;
using DatabaseOptionStatusLine = DataLayer.MongoData.Option.Status.OptionStatusLine;

namespace Administration.Option.Decider
{
	public class OptionDecider
	{
		private readonly MongoConnection _connection;
		private OptionStatus _optionStatus;

		public OptionDecider(MongoConnection connection, OptionStatus optionStatus)
		{
			_connection = connection;
			_optionStatus = optionStatus;
		}

		public OptionBase Decide(List<OptionBase> options)
		{
			if (options.Any(option => option.GetType().Name == "Email"))
			{
				return options.First(option => option.GetType().Name == "Email");
			}

			options = options.OrderByDescending(option => FindPrioityIndex(option)).ToList();

			return options.First();
		}

		public void DelayOptionFromFails(OptionBase option)
		{
			DatabaseOptionStatusLine line = _optionStatus.GetDatabaseOptionStatusLineOnOption(option);

			int failsThisHour = 0;

			if (line != null)
			{
				failsThisHour = line.Fail1Hour;
			}

			if (failsThisHour == 0)
			{
				return;
			}

			double minutesExtraDouble = 10.0 * Math.Log(failsThisHour);

			TimeSpan minutesExtra = TimeSpan.FromMinutes(minutesExtraDouble);

			option.DatabaseOption.Schedule.NextAllowedExecution += minutesExtra;
		}

		private double FindPrioityIndex(OptionBase option)
		{
			DatabaseOptionStatusLine line = _optionStatus.GetDatabaseOptionStatusLineOnOption(option);

			double priority = 1;

			if (line?.Fail10Minute > 0)
			{
				priority *= 0.5;
			}
			else if (line?.Fail1Hour > 0)
			{
				priority *= 0.75;
			}
			else if (line?.Fail24Hour > 0)
			{
				priority *= 0.85;
			}

			int? failsOnCurrentOption = option?.DatabaseOption?.Schedule?.Fails;

			if (failsOnCurrentOption != null)
			{
				priority *= 0.5;
			}

			return priority;
		}
	}
}
