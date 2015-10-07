using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseCsvRead = DataLayer.MongoData.Option.Options.Csv.CsvRead;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Csv
{
	public class CsvRead : OptionBase
	{
		private DatabaseCsvRead _databaseCsvRead;

		public CsvRead(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<CsvRead> Find(MongoConnection connection)
		{
			List<DatabaseCsvRead> databaseCsvReads = DatabaseOptionBase.ReadAllowed<DatabaseCsvRead>(connection);

			return databaseCsvReads.Select(databaseCsvRead => new CsvRead(connection, databaseCsvRead)
			{
				_databaseCsvRead = databaseCsvRead,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(_databaseCsvRead.Delimeter, _databaseCsvRead.FileName, _databaseCsvRead.FileNameTmp);
			
            csv.ReadFields(_databaseCsvRead.KeyName, _databaseCsvRead.KeyValue);

			return true;
		}
	}
}
