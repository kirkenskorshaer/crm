using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using DatabaseImportFromKKAdminToNewModel = DataLayer.MongoData.Option.Options.Logic.ImportFromKKAdminToNewModel;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class ImportFromKKAdminToNewModelTest : TestBase
	{
		[Test]
		public void ExecuteTest()
		{
			DateTime firstValidIndbetaling = DateTime.Now - TimeSpan.FromDays(365 * 5);

			DatabaseImportFromKKAdminToNewModel databaseImportFromKKAdminToNewModel = new DatabaseImportFromKKAdminToNewModel()
			{
				Name = "test",
				ReportFileName = "C:/import/test/report.csv",
				StamDataFileName = "C:/import/test/test_stamdata_alle.csv",
				TilknytningerFileName = "C:/import/test/test_tilknytninger.csv",
				IndbetalingerFileName = "C:/import/test/test_indbetalinger.csv",
				FirstValidIndbetaling = firstValidIndbetaling,
				Import = true,
				MaxNumberOfImports = 3,
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			ImportFromKKAdminToNewModel importFromKKAdminToNewModel = new ImportFromKKAdminToNewModel(Connection, databaseImportFromKKAdminToNewModel);

			importFromKKAdminToNewModel.ExecuteOption(new Administration.Option.Options.OptionReport(typeof(ExposeDataTest)));
		}
	}
}
