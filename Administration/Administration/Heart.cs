using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Administration.Option;
using Administration.Option.Decider;
using Administration.Option.Finder;
using DataLayer;
using DataLayer.MongoData;

namespace Administration
{
	public class Heart
	{
		private readonly OptionFinder _optionFinder;
		private readonly OptionDecider _optionDecider;
		private readonly MongoConnection _connection;

		public Heart()
		{
			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			_connection = MongoConnection.GetConnection(databaseName);
			_optionFinder = new OptionFinder(_connection);
			_optionDecider = new OptionDecider(_connection);
		}

		private bool _run = true;

		public void Run()
		{
			while (_run)
			{
				try
				{
					HeartBeat();
				}
				catch (Exception exception)
				{
					Log.Write(_connection, exception.Message, exception.StackTrace, Config.LogLevelEnum.HeartError);
				}
			}
		}

		public void Stop()
		{
			_run = false;
		}

		public void HeartBeat()
		{
			Log.Write(_connection, $"heartbeat at {DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm")}", Config.LogLevelEnum.HeartMessage);
			List<OptionBase> options = _optionFinder.Find();

			if (options.Any() == false)
			{
				_run = false;
			}

			OptionBase bestOption = _optionDecider.Decide(options);

			try
			{
				bool isSuccess = bestOption.Execute();

				if (isSuccess)
				{
					_optionDecider.MarkAsSuccess(bestOption);
				}
				else
				{
					_optionDecider.MarkAsFailiure(bestOption);
				}
			}
			catch (Exception exception)
			{
				Log.Write(_connection, exception.Message, exception.StackTrace, Config.LogLevelEnum.HeartError);
				_optionDecider.MarkAsFailiure(bestOption);
			}

			_optionDecider.Decrease();
		}
	}
}
