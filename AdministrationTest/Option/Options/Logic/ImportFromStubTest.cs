using Administration.Option.Options.Logic;
using NUnit.Framework;
using DatabaseStub = DataLayer.MongoData.Input.Stub;
using DatabaseStubPusher = DataLayer.MongoData.Input.StubPusher;
using DatabaseImportFromStub = DataLayer.MongoData.Option.Options.Logic.ImportFromStub;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;
using DatabaseStubElement = DataLayer.MongoData.Input.StubElement;
using SystemInterface.Dynamics.Crm;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class ImportFromStubTest : TestBase
	{
		Campaign _campaign;

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
		public void ImportFromStubImportsStub()
		{
			DatabaseWebCampaign webCampaign = CreateWebCampaign(_campaign, new List<string>() { });

			webCampaign.Insert(Connection);

			string firstname = $"firstname {Guid.NewGuid()}";

			DatabaseStub stub = CreateStub(webCampaign, firstname);
			DatabaseStubPusher.GetInstance(Connection).Push(stub);

			DatabaseImportFromStub databaseImportFromStub = CreateDatabaseImportStub(webCampaign);

			ImportFromStub importFromStub = new ImportFromStub(Connection, databaseImportFromStub);

			importFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Lead lead = Lead.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "firstname" }, new Dictionary<string, string>() { { "firstname", firstname } }).Single();
			lead.Delete();
		}

		[Test]
		public void StubWillBeImportedEvenIfItContainsInvalidData()
		{
			DatabaseWebCampaign webCampaign = CreateWebCampaign(_campaign, new List<string>() { "firstname" });

			webCampaign.Insert(Connection);

			string firstname = $"firstname {Guid.NewGuid()}";

			DatabaseStub stub = CreateStub(webCampaign, firstname);
			stub.Contents.AddRange(new List<DatabaseStubElement>()
			{
				new DatabaseStubElement() { Key = "lastname", Value = "test" },
				new DatabaseStubElement() { Key = "address1_line1", Value = "test" },
				new DatabaseStubElement() { Key = "address1_postalcode", Value = "test" },
				new DatabaseStubElement() { Key = "address1_city", Value = "test" },
				new DatabaseStubElement() { Key = "emailaddress1", Value = "test" },
				new DatabaseStubElement() { Key = "mobilephone", Value = "test" },
				new DatabaseStubElement() { Key = "new_oprindelseip", Value = "test" },
			});
			DatabaseStubPusher.GetInstance(Connection).Push(stub);

			DatabaseImportFromStub databaseImportFromStub = CreateDatabaseImportStub(webCampaign);

			ImportFromStub importFromStub = new ImportFromStub(Connection, databaseImportFromStub);

			importFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Lead lead = Lead.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "firstname" }, new Dictionary<string, string>() { { "firstname", firstname } }).Single();
			lead.Delete();
		}

		[Test]
		public void StubWillBeImportedWithoutCampaign()
		{
			string firstname = $"firstname {Guid.NewGuid()}";

			DatabaseStub stub = CreateStub(null, firstname);
			stub.Contents.AddRange(new List<DatabaseStubElement>()
			{
				new DatabaseStubElement() { Key = "lastname", Value = "test" },
				new DatabaseStubElement() { Key = "address1_line1", Value = "test" },
				new DatabaseStubElement() { Key = "address1_postalcode", Value = "test" },
				new DatabaseStubElement() { Key = "address1_city", Value = "test" },
				new DatabaseStubElement() { Key = "emailaddress1", Value = "test" },
				new DatabaseStubElement() { Key = "mobilephone", Value = "test" },
			});
			DatabaseStubPusher.GetInstance(Connection).Push(stub);

			DatabaseImportFromStub databaseImportFromStub = CreateDatabaseImportStub();

			ImportFromStub importFromStub = new ImportFromStub(Connection, databaseImportFromStub);

			importFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Lead lead = Lead.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "firstname" }, new Dictionary<string, string>() { { "firstname", firstname } }).Single();
			lead.Delete();
		}

		[Test]
		public void ImportFromStubImportsStubAndCreatesContact()
		{
			DatabaseWebCampaign webCampaign = CreateWebCampaign(_campaign, new List<string>() { "emailaddress1", "mobilephone" });

			webCampaign.Insert(Connection);

			DatabaseStub stub = CreateStub(webCampaign);
			string emailaddress1 = $"emailaddress1 {Guid.NewGuid()}";
			stub.Contents.Add(new DatabaseStubElement() { Key = "emailaddress1", Value = emailaddress1 });
			DatabaseStubPusher.GetInstance(Connection).Push(stub);

			DatabaseImportFromStub databaseImportFromStub = CreateDatabaseImportStub(webCampaign);

			ImportFromStub importFromStub = new ImportFromStub(Connection, databaseImportFromStub);

			importFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Lead lead = Lead.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "emailaddress1" }, new Dictionary<string, string>() { { "emailaddress1", emailaddress1 } }).Single();
			lead.Delete();

			Contact contact = Contact.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "emailaddress1" }, new Dictionary<string, string>() { { "emailaddress1", emailaddress1 } }).Single();
			contact.Delete();
		}

		[Test]
		public void ImportFromStubAssignsContactToAccount()
		{
			Account account = CreateAccount();
			account.InsertWithoutRead();

			DatabaseWebCampaign webCampaign = CreateWebCampaign(_campaign, new List<string>() { "emailaddress1" });

			webCampaign.Insert(Connection);

			DatabaseStub stub = CreateStub(webCampaign);
			string emailaddress1 = $"emailaddress1 {Guid.NewGuid()}";
			stub.Contents.Add(new DatabaseStubElement() { Key = "emailaddress1", Value = emailaddress1 });
			stub.Contents.Add(new DatabaseStubElement() { Key = "indsamler2016", Value = account.Id.ToString() });
			DatabaseStubPusher.GetInstance(Connection).Push(stub);

			DatabaseImportFromStub databaseImportFromStub = CreateDatabaseImportStub(webCampaign);

			ImportFromStub importFromStub = new ImportFromStub(Connection, databaseImportFromStub);

			importFromStub.ExecuteOption(new Administration.Option.Options.OptionReport(""));

			Lead lead = Lead.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "emailaddress1" }, new Dictionary<string, string>() { { "emailaddress1", emailaddress1 } }).Single();
			Contact contact = Contact.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "contactid", "emailaddress1", "new_indsamler2016" }, new Dictionary<string, string>() { { "emailaddress1", emailaddress1 } }).Single();
			Account accountRead = Account.ReadFromFetchXml(DynamicsCrmConnection, new List<string>() { "accountid" }, new Dictionary<string, string>() { { "accountid", contact.indsamler2016.ToString() } }).Single();

			lead.Delete();
			contact.Delete();
			accountRead.Delete();
		}

		private DatabaseImportFromStub CreateDatabaseImportStub(DatabaseWebCampaign webCampaign)
		{
			DatabaseImportFromStub databaseImportFromStub = CreateDatabaseImportStub();
			databaseImportFromStub.WebCampaignId = webCampaign._id;

			return databaseImportFromStub;
		}

		private DatabaseImportFromStub CreateDatabaseImportStub()
		{
			return new DatabaseImportFromStub()
			{
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
				Name = "test",
			};
		}

		private DatabaseWebCampaign CreateWebCampaign(Campaign campaign, List<string> KeyFields)
		{
			return new DatabaseWebCampaign()
			{
				CollectType = DatabaseWebCampaign.CollectTypeEnum.LeadOgContactHvisContactIkkeFindes,
				FormId = campaign.Id,
				RedirectTarget = "localhost",
				KeyFields = KeyFields,
			};
		}
	}
}
