﻿using System;
using DataLayer;
using System.Threading;
using Administration.Option.Options;
using DatabaseOptionStatus = DataLayer.MongoData.Option.Status.OptionStatus;
using DatabaseOptionStatusLine = DataLayer.MongoData.Option.Status.OptionStatusLine;
using DatabaseOptionResult = DataLayer.MongoData.Option.Status.OptionResult;
using DatabaseWorker = DataLayer.MongoData.Worker;
using System.Collections.Generic;
using System.Diagnostics;

namespace Administration.Option.Status
{
	public class OptionStatus
	{
		private MongoConnection _mongoConnection;
		private object _statusLock = new object();
		private Timer _databaseUpdater;
		private DatabaseOptionStatus _databaseOptionStatus;

		private TimeSpan _writeInterval = TimeSpan.FromSeconds(60);

		private DateTime _lastWriteTime = DateTime.MinValue;

		public OptionStatus(MongoConnection mongoConnection, TimeSpan writeInterval)
		{
			_mongoConnection = mongoConnection;

			_databaseOptionStatus = DatabaseOptionStatus.ReadOrCreate(_mongoConnection);

			_writeInterval = writeInterval;

			_databaseUpdater = new Timer(WriteToDatabase, null, _writeInterval, _writeInterval);
		}

		public DatabaseOptionStatusLine GetDatabaseOptionStatusLineOnOption(OptionBase option)
		{
			lock (_statusLock)
			{
				string optionName = option.GetType().Name;

				if (_databaseOptionStatus.options.ContainsKey(optionName))
				{
					DatabaseOptionStatusLine line = _databaseOptionStatus.options[optionName];
					return line;
				}
			}

			return null;
		}

		public void SetWriteInterval(TimeSpan writeInterval)
		{
			_writeInterval = writeInterval;

			_databaseUpdater.Change(_writeInterval, _writeInterval);
		}

		public void BeginOption(OptionBase currentOption, DatabaseWorker databaseWorker)
		{
			lock (_statusLock)
			{
				_databaseOptionStatus.ActiveOptionsAdd(currentOption.DatabaseOption, databaseWorker);
			}
		}

		public void EndOption(OptionBase currentOption, OptionReport report)
		{
			lock (_statusLock)
			{
				_databaseOptionStatus.ActiveOptionsRemove(currentOption.DatabaseOption);

				if (report != null)
				{
					DatabaseOptionResult.Create(_mongoConnection, report.BeginTime, report.EndTime, report.OptionType.Name, report.Success, report.Memory);

					UpdateEstimatedStatistics(report);

					if (_databaseOptionStatus.optionLastMemory.ContainsKey(report.OptionType.Name))
					{
						_databaseOptionStatus.optionLastMemory[report.OptionType.Name] = report.Memory;
					}
					else
					{
						_databaseOptionStatus.optionLastMemory.Add(report.OptionType.Name, report.Memory);
					}
				}
			}
		}

		private void UpdateEstimatedStatistics(OptionReport report)
		{
			lock (_statusLock)
			{
				string optionName = report.OptionType.Name;

				DatabaseOptionStatusLine line = null;

				if (_databaseOptionStatus.options.ContainsKey(optionName))
				{
					line = _databaseOptionStatus.options[optionName];
				}
				else
				{
					line = new DatabaseOptionStatusLine();
					_databaseOptionStatus.options.Add(optionName, line);
				}

				line.ExecutionTotal++;
				if (report.Success)
				{
					line.SuccessTotal++;
					line.Success10Minute++;
				}
				else
				{
					line.FailTotal++;
					line.Fail10Minute++;
				}
			}
		}

		private void WriteToDatabase(object state)
		{
			lock (_statusLock)
			{
				Process currentProcess = Process.GetCurrentProcess();

				_databaseOptionStatus.MainMemory = currentProcess.WorkingSet64;

				_databaseOptionStatus.UpdateStatisticsFromResults(_mongoConnection);

				_databaseOptionStatus.UpdateStatus(_mongoConnection);
			}
		}

		internal void RemoveOptionsFromDeadWorkers(List<DatabaseWorker> deadWorkers)
		{
			lock (_statusLock)
			{
				_databaseOptionStatus.ActiveOptionsRemoveByWorkerList(deadWorkers);
			}
		}
	}
}
