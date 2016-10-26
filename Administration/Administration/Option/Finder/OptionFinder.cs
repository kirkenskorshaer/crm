using System.Collections.Generic;
using Administration.Option.Options;
using Administration.Option.Options.Service;
using DataLayer;
using Administration.Option.Options.Csv;
using Administration.Option.Options.Logic;
using Administration.Option.Options.Data.Utilities;

namespace Administration.Option.Finder
{
	public class OptionFinder
	{
		private readonly MongoConnection _connection;
		public OptionFinder(MongoConnection connection)
		{
			_connection = connection;
		}

		public List<OptionBase> Find()
		{
			List<OptionBase> options = new List<OptionBase>();
			/*
			options.AddRange(Email.Find(_connection));

			options.AddRange(ServiceCreate.Find(_connection));
			options.AddRange(ServiceDelete.Find(_connection));
			options.AddRange(ServiceStart.Find(_connection));
			options.AddRange(ServiceStop.Find(_connection));

			options.AddRange(CsvWriteLine.Find(_connection));
			options.AddRange(CsvRead.Find(_connection));
			options.AddRange(CsvUpdate.Find(_connection));
			options.AddRange(CsvDelete.Find(_connection));

			options.AddRange(MaintainProgress.Find(_connection));
			options.AddRange(Squash.Find(_connection));
			options.AddRange(StressTestCrm.Find(_connection));
			options.AddRange(SynchronizeFromCrm.Find(_connection));
			options.AddRange(SynchronizeFromCsv.Find(_connection));
			options.AddRange(SynchronizeToCrm.Find(_connection));
			options.AddRange(AdjustDns.Find(_connection));
			*/
			options.AddRange(ImportFromStub.Find(_connection));
			options.AddRange(CreateImportFromStub.Find(_connection));
			options.AddRange(MaterialeBehovAssignment.Find(_connection));
			options.AddRange(ExposeData.Find(_connection));
			options.AddRange(AddMailrelaySubscriberFromLead.Find(_connection));
			options.AddRange(UpdateMailrelayFromContact.Find(_connection));
			options.AddRange(UpdateMailrelayGroup.Find(_connection));
			options.AddRange(ExportContactToMailrelay.Find(_connection));
			options.AddRange(SendTableFromMailrelay.Find(_connection));
			options.AddRange(ImportDanskeBank.Find(_connection));
			options.AddRange(SumIndbetaling.Find(_connection));
			//options.AddRange(MaintainAllTables.Find(_connection));

			Sleep sleep = new Sleep(_connection);
			options.Add(sleep);

			return options;
		}
	}
}