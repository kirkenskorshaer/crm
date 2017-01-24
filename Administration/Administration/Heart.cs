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
using DatabaseOptionResult = DataLayer.MongoData.Option.Status.OptionResult;

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

		private TimeSpan _clearOptionResultsOlderThan;
		private TimeSpan _clearOptionResultsInterval;
		private TimeSpan _statusWriteInterval = TimeSpan.FromSeconds(5);

		private TimeSpan _reloadConfigInterval;

		private DateTime _lastReloadConfig = DateTime.MinValue;
		private DateTime _lastClearOptionResults = DateTime.MinValue;
		private DateTime _lastCheckForDeadWorkers = DateTime.MinValue;

		private Config _config;

		private DateTime _startTime;

		public Heart()
		{
			//Log.FileWrite(GetType().Name, "Start Initialize");

			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			_connection = MongoConnection.GetConnection(databaseName);
			_optionFinder = new OptionFinder(_connection);

			_optionStatus = new OptionStatus(_connection, _statusWriteInterval);

			//Log.FileWrite(GetType().Name, "End Initialize");
		}

		private void ReloadConfig()
		{
			if (_lastReloadConfig + _reloadConfigInterval > Clock.Now)
			{
				return;
			}

			_config = Config.GetConfig(_connection);
			Log.LogLevel = _config.LogLevel;

			//Log.FileWrite(GetType().Name, "Config read start");

			TimeSpan NewStatusWriteInterval = TimeSpan.FromSeconds(_config.StatusWriteIntervalSeconds);
			if (_statusWriteInterval.CompareTo(NewStatusWriteInterval) != 0)
			{
				_statusWriteInterval = NewStatusWriteInterval;
				_optionStatus.SetWriteInterval(_statusWriteInterval);
			}

			_heartSleep = TimeSpan.FromMilliseconds(_config.HeartSleepMilliseconds);
			_timeToWaitForWorkers = TimeSpan.FromMinutes(_config.AsumeWorkerIsDeadIfIdleForMinutes);
			_timeToWaitBetweenChecksForDeadWorkers = TimeSpan.FromMinutes(_config.TimeToWaitBetweenChecksForDeadWorkersMinutes);
			_clearOptionResultsOlderThan = TimeSpan.FromHours(_config.ClearOptionResultsOlderThanHours);
			_clearOptionResultsInterval = TimeSpan.FromHours(_config.ClearOptionResultsIntervalHours);

			_reloadConfigInterval = TimeSpan.FromMinutes(_config.ReloadConfigIntervalMinutes);

			_lastReloadConfig = Clock.Now;

			//Log.FileWrite(GetType().Name, "Config read done");
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
				//Log.FileWrite(GetType().Name, "Heartbeat...");
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

			ReloadConfig();

			ClearOldOptionResults();

			AdjustThreadHolderCount();

			_optionFinder.DistributeOptions(_threadHolders);

			UnassignFromDeadWorkers();

			_threadHolders.ForEach(threadHolder => threadHolder.HeartBeat());

			Thread.Sleep(_heartSleep);
		}

		private void ClearOldOptionResults()
		{
			if (_lastClearOptionResults + _clearOptionResultsInterval > Clock.Now)
			{
				return;
			}

			DateTime maxAllowedEndDate = Clock.Now - _clearOptionResultsOlderThan;

			DatabaseOptionResult.ClearOldResults(_connection, maxAllowedEndDate);

			_lastClearOptionResults = Clock.Now;
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
