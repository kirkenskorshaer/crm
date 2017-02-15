using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseExposeData = DataLayer.MongoData.Option.Options.Logic.ExposeData;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class ExposeDataTest : TestBase
	{
		[Test]
		[Ignore("")]
		public void ExposeDataExposesDataInFile()
		{
			DatabaseExposeData databaseExposeData = new DatabaseExposeData()
			{
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
				fetchXmlPath = "Dynamics/Crm/FetchXml/Account/plant.xml",
				exposePath = "webinterface",
				exposeName = "test.txt",
			};

			ExposeData exposeData = new ExposeData(Connection, databaseExposeData);

			exposeData.ExecuteOption(new Administration.Option.Options.OptionReport(typeof(ExposeDataTest)));
		}
	}
}
