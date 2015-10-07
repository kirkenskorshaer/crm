using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseCsvDelete = DataLayer.MongoData.Option.Options.Csv.CsvDelete;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Csv
{
	public class CsvDelete : OptionBase
	{
		private DatabaseCsvDelete _databaseCsvDelete;

		public CsvDelete(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<CsvDelete> Find(MongoConnection connection)
		{
			List<DatabaseCsvDelete> databaseCsvDeletes = DatabaseOptionBase.ReadAllowed<DatabaseCsvDelete>(connection);

			return databaseCsvDeletes.Select(databaseCsvDelete => new CsvDelete(connection, databaseCsvDelete)
			{
				_databaseCsvDelete = databaseCsvDelete,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(_databaseCsvDelete.Delimeter, _databaseCsvDelete.FileName, _databaseCsvDelete.FileNameTmp);

			csv.Delete(_databaseCsvDelete.KeyName, _databaseCsvDelete.KeyValue);

			return true;
		}
	}
}
