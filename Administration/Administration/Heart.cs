using System;
using System.Collections.Generic;
using System.Configuration;
using Administration.Option;
using Administration.Option.Finder;
using DataLayer;
using DataLayer.MongoData;
using Administration.Option.Status;
using System.Threading;
using Utilities;
using System.Linq;
using DatabaseWorker = DataLayer.MongoData.Worker;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration
{
	public class Heart
	{
		private readonly OptionFinder _optionFinder;
		private readonly MongoConnection _connection;
		private readonly OptionStatus _optionStatus;
		private List<ThreadHolder> _threadHolders = new List<ThreadHolder>();
		private TimeSpan _heartSleep = TimeSpan.FromMilliseconds(500);
		private TimeSpan _timeToWaitForWorkers = TimeSpan.FromHours(1);
		private TimeSpan _timeToWaitBetweenChecksForDeadWorkers = TimeSpan.FromMinutes(10);
		private DateTime _lastCheckForDeadWorkers = DateTime.MinValue;
		private Config _config;

		private DateTime _startTime;

		public Heart()
		{
			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			_connection = MongoConnection.GetConnection(databaseName);
			_optionFinder = new OptionFinder(_connection);
			_config = Config.GetConfig(_connection);
			Log.LogLevel = _config.LogLevel;

			TimeSpan StatusWriteInterval = TimeSpan.FromSeconds(_config.StatusWriteIntervalSeconds);
			_heartSleep = TimeSpan.FromMilliseconds(_config.HeartSleepMilliseconds);
			_timeToWaitForWorkers = TimeSpan.FromMinutes(_config.AsumeWorkerIsDeadIfIdleForMinutes);
			_timeToWaitBetweenChecksForDeadWorkers = TimeSpan.FromMinutes(_config.TimeToWaitBetweenChecksForDeadWorkersMinutes);

			_optionStatus = new OptionStatus(_connection, StatusWriteInterval);
		}

		private bool _run = true;

		public void Begin()
		{
			_startTime = Clock.Now;
			Log.WriteLocation(_connection, $"starting", "Heart", Config.LogLevelEnum.HeartMessage);
		}

		public void Run()
		{
			Begin();

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

			End();
		}

		public void End()
		{
			_threadHolders.ForEach(threadHolder => threadHolder.StopThread());

			DateTime endTime = Clock.Now;
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

			AdjustThreadHolderCount();

			_optionFinder.DistributeOptions(_threadHolders);

			UnassignFromDeadWorkers();

			_threadHolders.ForEach(threadHolder => threadHolder.HeartBeat());

			Thread.Sleep(_heartSleep);
		}

		public bool IsAnyThreadHolderBusy()
		{
			return _threadHolders.Any(threadHolder => threadHolder.IsBusy());
		}

		public bool IsWorkQueued()
		{
			return _threadHolders.Any(threadHolder => threadHolder.IsWorkQueued());
		}

		private void UnassignFromDeadWorkers()
		{
			if (_lastCheckForDeadWorkers + _timeToWaitBetweenChecksForDeadWorkers > Clock.Now)
			{
				return;
			}

			List<DatabaseWorker> deadWorkers = GetDeadWorkers();

			deadWorkers.ForEach(worker => DatabaseOptionBase.UnAssignWorkerFromAllAssigned(worker, _connection));

			_optionStatus.RemoveOptionsFromDeadWorkers(deadWorkers);

			_lastCheckForDeadWorkers = Clock.Now;
		}

		private List<DatabaseWorker> GetDeadWorkers()
		{
			DateTime lastAcceptableWorkerTime = Clock.Now - _timeToWaitForWorkers;

			List<DatabaseWorker> deadWorkers = DatabaseWorker.GetDeadWorkers(_connection, lastAcceptableWorkerTime);

			deadWorkers = deadWorkers.Where(worker => _threadHolders.Any(threadHolder => threadHolder.DatabaseWorker.Id == worker.Id) == false).ToList();

			return deadWorkers;
		}

		private void AdjustThreadHolderCount()
		{
			while (_threadHolders.Count < _config.Threads)
			{
				ThreadHolder threadHolder = new ThreadHolder(_connection, _optionStatus);
				_threadHolders.Add(threadHolder);
			}

			while (_threadHolders.Count > _config.Threads)
			{
				ThreadHolder doomedThreadHolder = _threadHolders.Last();
				doomedThreadHolder.Doom(_connection);

				_threadHolders.Remove(doomedThreadHolder);
			}
		}
	}
}
