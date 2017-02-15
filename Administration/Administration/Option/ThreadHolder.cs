using Administration.Option.Decider;
using Administration.Option.Finder;
using Administration.Option.Options;
using Administration.Option.Status;
using DataLayer;
using DataLayer.MongoData;
using System;
using System.Collections.Generic;
using System.Threading;
using DatabaseWorker = DataLayer.MongoData.Worker;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using Utilities;
using System.Linq;

namespace Administration.Option
{
	public class ThreadHolder
	{
		private MongoConnection _mongoConnection;
		public DatabaseWorker DatabaseWorker { get; private set; }
		private Thread _thread;
		private readonly OptionFinder _optionFinder;
		private readonly OptionStatus _optionStatus;
		private readonly OptionDecider _optionDecider;
		private bool _isDoomed = false;
		private DateTime _lastUpdatedLastWorkTime = DateTime.MinValue;
		private TimeSpan _updateLastWorkTimeMinimumInterval = TimeSpan.FromMinutes(10);

		private object threadHolderLock = new object();

		private int _estimatedOptionCount;
		public int EstimatedOptionCount
		{
			get
			{
				return _estimatedOptionCount;
			}
			set
			{
				lock (threadHolderLock)
				{
					_estimatedOptionCount = value;
				}
			}
		}

		public ThreadHolder(MongoConnection mongoConnection, OptionStatus optionStatus)
		{
			_mongoConnection = mongoConnection;

			DatabaseWorker = new DatabaseWorker();
			DatabaseWorker.Create(_mongoConnection);
			_lastUpdatedLastWorkTime = Clock.Now;

			_optionFinder = new OptionFinder(_mongoConnection);
			_optionStatus = optionStatus;
			_optionDecider = new OptionDecider(_mongoConnection, _optionStatus);
		}

		public void HeartBeat()
		{
			SetLastWorkTime();

			if (_thread != null && _thread.ThreadState != ThreadState.Stopped)
			{
				return;
			}

			List<OptionBase> options = _optionFinder.Find(DatabaseWorker);

			EstimatedOptionCount = options.Count;

			if (options.Count == 0 && _isDoomed == false)
			{
				return;
			}

			if (_isDoomed)
			{
				StopThread();

				return;
			}

			OptionBase currentOption = _optionDecider.Decide(options);

			_thread = new Thread(RunThread);

			_thread.Start(currentOption);
		}

		private void SetLastWorkTime()
		{
			if (_lastUpdatedLastWorkTime + _updateLastWorkTimeMinimumInterval > Clock.Now)
			{
				return;
			}

			DatabaseWorker.LastWorkTime = Clock.Now;
			DatabaseWorker.Update(_mongoConnection);
		}

		public void RunThread(object currentOptionObject)
		{
			OptionBase currentOption = (OptionBase)currentOptionObject;

			_optionStatus.BeginOption(currentOption, DatabaseWorker);

			OptionReport report = new OptionReport(currentOption.GetType());

			try
			{
				currentOption.ExecuteOption(report);

				_optionDecider.DelayOptionFromFails(currentOption);

				currentOption.DatabaseOption?.Execute(_mongoConnection);
			}
			catch (Exception exception)
			{
				Log.Write(_mongoConnection, exception.Message, typeof(ThreadHolder), exception.StackTrace, Config.LogLevelEnum.HeartError);

				_optionDecider.DelayOptionFromFails(currentOption);

				currentOption.DatabaseOption?.ExecuteFail(_mongoConnection);
			}
			finally
			{
				report.FinishReport();

				report.WriteLog(_mongoConnection);

				_optionStatus.EndOption(currentOption, report);
			}
		}

		internal bool IsBusy()
		{
			if (_thread == null)
			{
				return false;
			}

			if (_thread.ThreadState != ThreadState.Stopped)
			{
				return true;
			}

			return false;
		}

		internal bool IsWorkQueued()
		{
			List<OptionBase> options = _optionFinder.Find(DatabaseWorker);

			return options.Any();
		}

		public void StopThread()
		{
			DatabaseOptionBase.UnAssignWorkerFromAllAssigned(DatabaseWorker, _mongoConnection);

			DatabaseWorker.Delete(_mongoConnection);

			DatabaseWorker = null;
		}

		public void Doom(MongoConnection _connection)
		{
			_isDoomed = true;
		}
	}
}
