using DataLayer;
using System.Collections.Generic;
using System.Linq;
using DatabaseCsvWriteLine = DataLayer.MongoData.Option.Options.Csv.CsvWriteLine;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace Administration.Option.Options.Csv
{
	public class CsvWriteLine : OptionBase
	{
		private DatabaseCsvWriteLine _databaseCsvWriteLine;

		public CsvWriteLine(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
		}

		public static List<CsvWriteLine> Find(MongoConnection connection)
		{
			List<DatabaseCsvWriteLine> databaseCsvWriteLines = DatabaseOptionBase.ReadAllowed<DatabaseCsvWriteLine>(connection);

			return databaseCsvWriteLines.Select(databaseCsvWriteLine => new CsvWriteLine(connection, databaseCsvWriteLine)
			{
				_databaseCsvWriteLine = databaseCsvWriteLine,
			}).ToList();
		}

		protected override bool ExecuteOption()
		{
			SystemInterface.Csv.ColumnDefinition[] columns = _databaseCsvWriteLine.CsvElements.Select(csvElement => new SystemInterface.Csv.ColumnDefinition(SystemInterface.Csv.ColumnDefinition.DataTypeEnum.stringType, csvElement.Key)).ToArray();
			string[] values = _databaseCsvWriteLine.CsvElements.Select(csvElement => csvElement.Value).ToArray();

			SystemInterface.Csv.Csv csv = new SystemInterface.Csv.Csv(_databaseCsvWriteLine.Delimeter, _databaseCsvWriteLine.FileName, _databaseCsvWriteLine.FileNameTmp, columns);

			csv.WriteLine(values);

			return true;
		}
	}
}
