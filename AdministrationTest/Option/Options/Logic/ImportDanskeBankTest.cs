using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseImportDanskeBank = DataLayer.MongoData.Option.Options.Logic.ImportDanskeBank;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class ImportDanskeBankTest : TestBase
	{
		[Test]
		public void test()
		{
			DatabaseImportDanskeBank databaseImportDanskeBank = CreateDatabaseImportDanskeBank();

			ImportDanskeBank importDanskeBank = new ImportDanskeBank(Connection, databaseImportDanskeBank);

			importDanskeBank.Execute();
		}

		private DatabaseImportDanskeBank CreateDatabaseImportDanskeBank()
		{
			return new DatabaseImportDanskeBank()
			{
				importFolder = "danskebank",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};
		}
	}
}
