using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using DatabaseSendSms = DataLayer.MongoData.Option.Options.Logic.SendSms;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SendSmsTest : TestBase
	{
		[TearDown]
		new public void TearDown()
		{
			base.TearDown();

			Console.Out.WriteLine(_inMobileConnectionTester);
		}

		[Test]
		public void Send()
		{
			DatabaseSendSms databaseSendSms = CreateDatabaseSendSms();
			SendSms sendSms = new SendSms(Connection, databaseSendSms);

			sendSms.SetDynamicsCrmConnectionIfEmpty(_dynamicsCrmConnectionTester);
			EnqueueCrmSmsTemplateResponse();
			EnqueueCrmSmsResponse();
			EnqueueCrmContactResponse();

			sendSms.SetInMobileConnectionIfEmpty(_inMobileConnectionTester);

			sendSms.Execute();
		}

		[Test]
		public void SendFromCrm()
		{
			DatabaseSendSms databaseSendSms = CreateDatabaseSendSms();
			SendSms sendSms = new SendSms(Connection, databaseSendSms);

			sendSms.SetInMobileConnectionIfEmpty(_inMobileConnectionTester);

			sendSms.Execute();
		}

		private DatabaseSendSms CreateDatabaseSendSms()
		{
			DatabaseSendSms databaseSendSms = new DatabaseSendSms()
			{
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
				urlLoginName = "test",
			};

			return databaseSendSms;
		}

		private void EnqueueCrmSmsTemplateResponse()
		{
			Dictionary<string, object> contactDictionary = new Dictionary<string, object>()
			{
				{ "new_text", "new_text" },
				{ "new_smstemplateid", Guid.NewGuid() },
				{ "new_fetchxml", "<test></test>" },
			};

			_dynamicsCrmConnectionTester.EnqueueRetrieveMultiple("new_smstemplate", new List<Dictionary<string, object>>() { contactDictionary });
		}

		private void EnqueueCrmSmsResponse()
		{
			Dictionary<string, object> contactDictionary = new Dictionary<string, object>()
			{
				{ "new_smsid", Guid.NewGuid() },
				{ "contactid", Guid.NewGuid() },
				{ "mobilephone", "12 34 56 78" },
				{ "new_sendtime", new DateTime(2000, 1, 1) },
				{ "new_operatorsendtime", new DateTime(2000, 1, 2) },
			};

			_dynamicsCrmConnectionTester.EnqueueRetrieveMultiple("new_sms", new List<Dictionary<string, object>>() { contactDictionary });
		}

		private void EnqueueCrmContactResponse()
		{
			Dictionary<string, object> contactDictionary = new Dictionary<string, object>()
			{
				{ "firstname", "firstname" },
			};

			_dynamicsCrmConnectionTester.EnqueueRetrieveMultiple("contact", new List<Dictionary<string, object>>() { contactDictionary });
		}
	}
}
