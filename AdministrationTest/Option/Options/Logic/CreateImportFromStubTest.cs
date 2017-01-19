using Administration.Option.Options.Logic;
using NUnit.Framework;
using System.Collections.Generic;
using DatabaseCreateImportFromStub = DataLayer.MongoData.Option.Options.Logic.CreateImportFromStub;
using DatabaseImportFromStub = DataLayer.MongoData.Option.Options.Logic.ImportFromStub;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using SystemInterface.Dynamics.Crm;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class CreateImportFromStubTest : TestBase
	{
		private Campaign _campaign;

		[SetUp]
		public new void SetUp()
		{
			base.SetUp();

			_campaign = CreateCampaign();
			_campaign.InsertWithoutRead();
		}

		[TearDown]
		public new void TearDown()
		{
			base.TearDown();

			_campaign.Delete();
		}

		[Test]
		public void ImportFromStubIsCreatedForCampaign()
		{
			DatabaseCreateImportFromStub databaseCreateImportFromStub = CreateDatabaseCreateImportFromStub();

			CreateImportFromStub createImportFromStub = new CreateImportFromStub(Connection, databaseCreateImportFromStub);

			createImportFromStub.ExecuteOption(new Administration.Option.Options.OptionReport("test"));

			DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadSingleOrDefault(Connection, _campaign.Id);

			List<DatabaseImportFromStub> importFromStubs = DatabaseImportFromStub.ReadByWebCampaign(Connection, webCampaign);

			Assert.AreEqual(1, importFromStubs.Count);
		}

		[Test]
		public void OnlyOneCreateImportIsCreatedForEachCampaign()
		{
			DatabaseCreateImportFromStub databaseCreateImportFromStub = CreateDatabaseCreateImportFromStub();

			CreateImportFromStub createImportFromStub = new CreateImportFromStub(Connection, databaseCreateImportFromStub);

			createImportFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));
			createImportFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadSingleOrDefault(Connection, _campaign.Id);

			List<DatabaseImportFromStub> importFromStubs = DatabaseImportFromStub.ReadByWebCampaign(Connection, webCampaign);

			Assert.AreEqual(1, importFromStubs.Count);
		}

		[Test]
		[Ignore]
		public void InsertDatabaseCreateImportFromStub()
		{
			DatabaseCreateImportFromStub.Create(Connection, "test", "test", CreateScheduleAlwaysOnDoOnce(), CreateScheduleAlwaysOnDoOnce());
		}

		private DatabaseCreateImportFromStub CreateDatabaseCreateImportFromStub()
		{
			return new DatabaseCreateImportFromStub()
			{
				ImportFromStubSchedule = CreateScheduleAlwaysOnDoOnce(),
				Name = "Test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};
		}
	}
}
