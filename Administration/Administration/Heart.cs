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

		private DateTime _startTime;

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
			_startTime = DateTime.Now;
			Log.WriteLocation(_connection, $"starting", "Heart", Config.LogLevelEnum.HeartMessage);

			while (_run)
			{
				try
				{
					HeartBeat();
				}
				catch (Exception exception)
				{
					WriteException(exception, Config.LogLevelEnum.HeartError);
				}
			}

			DateTime endTime = DateTime.Now;
			TimeSpan runtime = endTime - _startTime;

			Log.WriteLocation(_connection, $"stopping, ran from {_startTime.ToString("yyyyMMdd HH:mm:ss")} to {endTime.ToString("yyyyMMdd HH:mm:ss")}, running time = {Math.Round(runtime.TotalSeconds, 0)} Seconds", "Heart", Config.LogLevelEnum.HeartMessage);
		}

		private void WriteException(Exception exception, Config.LogLevelEnum logLevel)
		{
			Log.Write(_connection, exception.Message, exception.StackTrace, logLevel);

			if (exception.InnerException != null)
			{
				WriteException(exception.InnerException, logLevel);
			}
		}

		public void Stop()
		{
			_run = false;
		}

		public void HeartBeat()
		{
			Log.Write(_connection, "heartbeat", Config.LogLevelEnum.HeartMessage);
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
