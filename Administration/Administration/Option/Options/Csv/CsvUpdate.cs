using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseCsvUpdate = DataLayer.MongoData.Option.Options.Csv.CsvUpdate;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Csv
{
	public class CsvUpdate : OptionBase
	{
		private DatabaseCsvUpdate _databaseCsvUpdate;

		public CsvUpdate(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<CsvUpdate> Find(MongoConnection connection)
		{
			List<DatabaseCsvUpdate> databaseCsvUpdates = DatabaseOptionBase.ReadAllowed<DatabaseCsvUpdate>(connection);

			return databaseCsvUpdates.Select(databaseCsvUpdate => new CsvUpdate(connection, databaseCsvUpdate)
			{
				_databaseCsvUpdate = databaseCsvUpdate,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			string[] columns = _databaseCsvUpdate.CsvElements.Select(csvElement => csvElement.Key).ToArray();
			string[] values = _databaseCsvUpdate.CsvElements.Select(csvElement => csvElement.Value).ToArray();

			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(_databaseCsvUpdate.Delimeter, _databaseCsvUpdate.FileName, _databaseCsvUpdate.FileNameTmp, columns);

			csv.Update(_databaseCsvUpdate.KeyName, _databaseCsvUpdate.KeyValue, values);

			return true;
		}
	}
}
