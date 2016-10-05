using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SystemInterface.Mailrelay.Function.GeneralFunctions;
using SystemInterface.Mailrelay.FunctionReply;
using TestUtilities;
using DatabaseSendTableFromMailrelay = DataLayer.MongoData.Option.Options.Logic.SendTableFromMailrelay;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SendTableFromMailrelayTest : TestBase
	{
		[SetUp]
		new public void SetUp()
		{
			base.SetUp();
		}

		[TearDown]
		new public void TearDown()
		{
			base.TearDown();

			Console.Out.WriteLine(_mailrelayConnectionTester);
		}

		[Test]
		public void InsertFilledReplacesFirstField()
		{
			string originalString =
				"<html like string><field1>{!f1a;f1b;default}</field1></html like string>";

			string expected =
				"<html like string><field1>f1aResult</field1></html like string>";

			Dictionary<string, object> fields = new Dictionary<string, object>()
			{
				{ "f1a" , "f1aResult" },
			};

			AssertFields(originalString, expected, fields);
		}

		[Test]
		public void InsertFilledReplacesMiddleField()
		{
			string originalString =
				"<html like string><field1>{!f1a;f1b;default}</field1></html like string>";

			string expected =
				"<html like string><field1>f1bResult</field1></html like string>";

			Dictionary<string, object> fields = new Dictionary<string, object>()
			{
				{ "f1b" , "f1bResult" },
			};

			AssertFields(originalString, expected, fields);
		}

		[Test]
		public void InsertFilledReplacesDefault()
		{
			string originalString =
				"<html like string><field1>{!f1a;f1b;default}</field1></html like string>";

			string expected =
				"<html like string><field1>default</field1></html like string>";

			Dictionary<string, object> fields = new Dictionary<string, object>();

			AssertFields(originalString, expected, fields);
		}

		[Test]
		public void InsertFilledReplacesBlankDefault()
		{
			string originalString =
				"<html like string><field1>{!f1a;f1b;}</field1></html like string>";

			string expected =
				"<html like string><field1></field1></html like string>";

			Dictionary<string, object> fields = new Dictionary<string, object>();

			AssertFields(originalString, expected, fields);
		}

		[Test]
		public void MailCanBeSendt()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			databaseSendTableFromMailrelay.limitOnDateName = "createdon";

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			sendTableFromMailrelay.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmResponse(new List<DateTime>() { new DateTime(2000, 1, 1), DateTime.Now, new DateTime(2000, 1, 2), new DateTime(2000, 1, 3), DateTime.Now.AddDays(-1) });

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply());

			sendTableFromMailrelay.Execute();

			sendMail sendMailFunction = (sendMail)_mailrelayConnectionTester.sendFunctions.Single(function => function is sendMail);

			Console.Out.WriteLine(sendMailFunction.html);
		}

		[Test]
		public void MailCanBeSendtOnSmtp()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			databaseSendTableFromMailrelay.limitOnDateName = "createdon";
			databaseSendTableFromMailrelay.sendType = DatabaseSendTableFromMailrelay.SendTypeEnum.Smtp;
			databaseSendTableFromMailrelay.fromEmail = "svend.l@kirkenskorshaer.dk";

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			sendTableFromMailrelay.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmResponse(new List<DateTime>() { new DateTime(2000, 1, 1), DateTime.Now, new DateTime(2000, 1, 2), new DateTime(2000, 1, 3), DateTime.Now.AddDays(-1) });

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply());
			SystemInterface.Email.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
			SystemInterface.Email.PickupDirectoryLocation = "c:\\test\\email";

			sendTableFromMailrelay.Execute();
		}

		[Test]
		public void MailWillNotBeSentIfLimitedOnDate()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			databaseSendTableFromMailrelay.limitOnDateName = "createdon";

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			sendTableFromMailrelay.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmResponse(new List<DateTime>() { new DateTime(2000, 1, 1), new DateTime(2000, 1, 2), new DateTime(2000, 1, 3) });

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply());

			sendTableFromMailrelay.Execute();

			sendMail sendMailFunction = (sendMail)_mailrelayConnectionTester.sendFunctions.SingleOrDefault(function => function is sendMail);

			Assert.IsNull(sendMailFunction);
		}

		[Test]
		public void LimitedMailWillBeSendtIfThereAreContactsYesterdayEvening()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			databaseSendTableFromMailrelay.limitOnDateName = "createdon";

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			sendTableFromMailrelay.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmResponse(new List<DateTime>() { new DateTime(2000, 1, 1), new DateTime(2000, 1, 2), DateTime.Now.Date.AddHours(-3) });

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply());

			sendTableFromMailrelay.Execute();

			sendMail sendMailFunction = (sendMail)_mailrelayConnectionTester.sendFunctions.Single(function => function is sendMail);

			Console.Out.WriteLine(sendMailFunction.html);
		}

		[Test]
		[Ignore]
		public void LiveTest()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			//databaseSendTableFromMailrelay.limitOnDateName = "createdon";
			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.Execute();
		}

		[Test]
		public void MissingEmailOrFullnameWillNotBreakExecute()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			databaseSendTableFromMailrelay.limitOnDateName = "createdon";

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			sendTableFromMailrelay.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmResponse(new List<DateTime>() { new DateTime(2000, 1, 1), new DateTime(2000, 1, 2), DateTime.Now.Date.AddHours(-3) }, false, false);

			_mailrelayConnectionTester.replies.Enqueue(new MailrelayBoolReply());

			sendTableFromMailrelay.Execute();
		}

		[Test]
		public void FailedSendWillNotRetry()
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();
			databaseSendTableFromMailrelay.limitOnDateName = "createdon";

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			sendTableFromMailrelay.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmResponse(new List<DateTime>() { new DateTime(2000, 1, 1), new DateTime(2000, 1, 2), DateTime.Now.Date.AddHours(-3) });

			bool succeded = sendTableFromMailrelay.Execute();

			Assert.IsTrue(succeded);
		}

		[Test]
		public void GenerateDatabase()
		{
			DatabaseSendTableFromMailrelay.Create
			(
				contactid: Guid.NewGuid(),
				html: GetHtml(),
				name: "test",
				matchidname: "new_indsamler2016",
				queryCreateTable: GetQueryCreateTable(),
				queryFindContacts: GetQueryFindContacts(),
				subject: "subject",
				tablerow: GetTablerow(),
				urlLoginName: "test",
				schedule: new DataLayer.MongoData.Option.Schedule()
				{
					Recurring = true,
					NextAllowedExecution = DateTime.Now,
					HoursOfDayToSkip = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 22, 23 },
					TimeBetweenAllowedExecutions = TimeSpan.FromHours(18),
				},
				packageid: 1,
				mailboxfromid: 4,
				mailboxreplyid: 4,
				mailboxreportid: 4,
				orderbyDescending: "createdon",
				headerDateFormat: "dd.MMMM",
				tableDateFormat: "dd/MM/yyyy",
				limitOnDateName: "createdon",
				requireDataOnDaysFromToday: new List<int>() { -1 },
				sleepTimeOnFailiure: TimeSpan.FromMinutes(10),
				mongoConnection: Connection
			);
		}

		private void EnqueueCrmResponse(List<DateTime> createdonList, bool setEmail = true, bool setFullname = true)
		{
			KeyValuePair<string, string> postal = GetTestData.GetPostalCity();

			Dictionary<string, object> contactDictionary = new Dictionary<string, object>()
			{
				{ "firstname", GetTestData.GetName() },
				{ "matchid", Guid.NewGuid() },
				{ "address1_city", postal.Value },
				{ "address1_line1", "address1_line1" },
				{ "name", GetTestData.GetName() },
				{ "address1_line2", "address1_line2" },
				{ "address1_postalcode", postal.Key },
			};

			if (setFullname)
			{
				contactDictionary.Add("fullname", "fullname");
			}

			if (setEmail)
			{
				contactDictionary.Add("emailaddress1", GetTestData.GetEmail());
			}

			_dynamicsCrmConnectionTester.EnqueueRetrieveMultiple("contact", new List<Dictionary<string, object>>() { contactDictionary });

			List<Dictionary<string, object>> indsamlereList = new List<Dictionary<string, object>>();

			int nameSize = 2;
			foreach (DateTime createdon in createdonList)
			{
				indsamlereList.Add(new Dictionary<string, object>()
				{
					{ "firstname", GetTestData.GetName(nameSize) },
					{ "lastname", GetTestData.GetName(nameSize) },
					{ "emailaddress1", GetTestData.GetEmail() },
					{ "mobilephone", GetTestData.GetPhone() },
					{ "createdon", createdon },
				});

				nameSize = nameSize + 5;
			}

			_dynamicsCrmConnectionTester.EnqueueRetrieveMultiple("contact", indsamlereList);
		}

		private void AssertFields(string originalString, string expected, Dictionary<string, object> fields)
		{
			DatabaseSendTableFromMailrelay databaseSendTableFromMailrelay = CreateDatabaseSendTableFromMailrelay();

			SendTableFromMailrelay sendTableFromMailrelay = new SendTableFromMailrelay(Connection, databaseSendTableFromMailrelay);
			sendTableFromMailrelay.ChangeMailrelayConnection(_mailrelayConnectionTester);

			string output = sendTableFromMailrelay.InsertFilledFields(originalString, fields, "yyyy-MM-dd");

			Assert.AreEqual(expected, output);
		}

		private DatabaseSendTableFromMailrelay CreateDatabaseSendTableFromMailrelay()
		{
			return new DatabaseSendTableFromMailrelay()
			{
				html = GetHtml(),
				Name = "test",
				matchidname = "new_indsamler2016",
				queryCreateTable = GetQueryCreateTable(),
				queryFindContacts = GetQueryFindContacts(),
				subject = "subject",
				tablerow = GetTablerow(),
				urlLoginName = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				packageid = 1,
				mailboxfromid = 2,
				mailboxreplyid = 3,
				mailboxreportid = 4,
				orderbyDescending = "createdon",
				headerDateFormat = "dd.MMMM",
				tableDateFormat = "dd/MM/yyyy",
				requireDataOnDaysFromToday = new List<int>() { -1 },
				sleepTimeOnFailiure = TimeSpan.FromSeconds(1),
				port = 465,
				sendType = DatabaseSendTableFromMailrelay.SendTypeEnum.Api,
			};
		}

		private string GetHtml()
		{
			StringBuilder htmlBuilder = new StringBuilder();
			htmlBuilder.Append("<html>");
			htmlBuilder.Append("<body>");
			htmlBuilder.Append("Kære {!firstname;koordinator}<br/>");
			htmlBuilder.Append("<br/>");
			htmlBuilder.Append("I har i går den {!Yesterday;} fået {!RowCountDay-1;0} ny(e). I alt har {!RowCount;0} indsamlere via danmarkmodfattigdom.dk tilmeldt sig jeres indsamlingssted:<br/>");
			htmlBuilder.Append("<br/>");
			htmlBuilder.Append("{!name;}<br/>");
			htmlBuilder.Append("{!address1_line1;}, {!address1_line2;}<br/>");
			htmlBuilder.Append("{!address1_postalcode;} {!address1_city;}<br/><br/>");
			htmlBuilder.Append("Husk at kontakte indsamlerne før den 27. november og aftale, hvornår de skal møde op på indsamlingsstedet og få udleveret indsamlingsbøsser og ruter.<br/><br/>");
			htmlBuilder.Append("Her er jeres oversigt over de indsamlere, der indtil videre har tilmeldt sig jeres indsamlingssted:<br/><br/>");
			htmlBuilder.Append("	<table frame=\"box\" border=\"1\" frame=\"hsides\" rules=\"all\" cellpadding=\"7\">");
			htmlBuilder.Append("	<tr><th>Dato</th><th>Fornavn</th><th>Efternavn</th><th>Email</th><th>Mobil</th></tr>");
			htmlBuilder.Append("{rows}");
			htmlBuilder.Append("	</table><br/><br/>");
			htmlBuilder.Append("Har du spørgsmål, er du altid velkommen til at kontakte os, enten ved at skrive til landsindsamling@kirkenskorshaer.dk eller ringe til 3312 1600.<br/>");
			htmlBuilder.Append("<br/>");
			htmlBuilder.Append("Med venlig hilsen<br/>");
			htmlBuilder.Append("<br/>");
			htmlBuilder.Append("Kirkens Korshær<br/>");
			htmlBuilder.Append("Christina Lyng Frandsen<br/>");
			htmlBuilder.Append("Landsindsamlingen<br/>");
			htmlBuilder.Append("</body>");
			htmlBuilder.Append("</html>");

			return htmlBuilder.ToString();
		}

		private string GetTablerow()
		{
			return "<tr><td>{!createdon;}</td><td>{!firstname;}</td><td>{!lastname;}</td><td>{!emailaddress1;}</td><td>{!mobilephone;}</td></tr>";
		}

		private string GetQueryFindContacts()
		{
			XDocument xDocument = new XDocument();

			xDocument.Add(
				new XElement("fetch",
					new XElement("entity", new XAttribute("name", "account"),
						new XElement("attribute", new XAttribute("name", "accountid"), new XAttribute("alias", "matchid")),
						new XElement("attribute", new XAttribute("name", "address1_city"), new XAttribute("alias", "address1_city")),
						new XElement("attribute", new XAttribute("name", "address1_line1"), new XAttribute("alias", "address1_line1")),
						new XElement("attribute", new XAttribute("name", "name"), new XAttribute("alias", "name")),
						new XElement("attribute", new XAttribute("name", "address1_line2"), new XAttribute("alias", "address1_line2")),
						new XElement("attribute", new XAttribute("name", "address1_postalcode"), new XAttribute("alias", "address1_postalcode")),
						new XElement("link-entity", new XAttribute("name", "contact"), new XAttribute("from", "contactid"), new XAttribute("to", "new_indsamlingskoordinatorid"), new XAttribute("link-type", "inner"),
							new XElement("attribute", new XAttribute("name", "fullname"), new XAttribute("alias", "fullname")),
							new XElement("attribute", new XAttribute("name", "firstname"), new XAttribute("alias", "firstname")),
							new XElement("attribute", new XAttribute("name", "emailaddress1"), new XAttribute("alias", "emailaddress1")),
							new XElement("filter",
								new XElement("condition", new XAttribute("attribute", "emailaddress1"), new XAttribute("operator", "not-null")),
								new XElement("condition", new XAttribute("attribute", "fullname"), new XAttribute("operator", "not-null"))
							)
						)
					)
				)
			);

			return xDocument.ToString();
		}

		private string GetQueryCreateTable()
		{
			XDocument xDocument = new XDocument();

			xDocument.Add(
				new XElement("fetch", new XAttribute("aggregate", "true"),
					new XElement("entity", new XAttribute("name", "contact"),
						new XElement("attribute", new XAttribute("name", "firstname"), new XAttribute("alias", "firstname"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "mobilephone"), new XAttribute("alias", "mobilephone"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "lastname"), new XAttribute("alias", "lastname"), new XAttribute("groupby", "true")),
						new XElement("attribute", new XAttribute("name", "emailaddress1"), new XAttribute("alias", "emailaddress1"), new XAttribute("groupby", "true")),
						new XElement("link-entity", new XAttribute("name", "lead"), new XAttribute("from", "parentcontactid"), new XAttribute("to", "contactid"), new XAttribute("link-type", "outer"),
							new XElement("attribute", new XAttribute("name", "createdon"), new XAttribute("alias", "createdon"), new XAttribute("aggregate", "max")),
							new XElement("filter",
								new XElement("condition", new XAttribute("attribute", "campaignid"), new XAttribute("operator", "eq"), new XAttribute("value", "ff052597-5538-e611-80ef-001c4215c4a0"))
							)
						)
					)
				)
			);

			return xDocument.ToString();
		}
	}
}
