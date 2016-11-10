using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Inmobile;
using Utilities;
using DatabaseSendSms = DataLayer.MongoData.Option.Options.Logic.SendSms;
using TwilioMessageInfo = SystemInterface.Twilio.MessageInfo;

namespace Administration.Option.Options.Logic
{
	public class SendSms : AbstractReportingDataOptionBase
	{
		private DatabaseSendSms _databaseSendSms;

		public SendSms(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSendSms = (DatabaseSendSms)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseSendSms.urlLoginName;
			string fromNumber = _databaseSendSms.fromNumber;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			SmsTemplate template = SmsTemplate.GetWaitingTemplate(_dynamicsCrmConnection);

			SetInMobileConnectionIfEmpty();
			IEnumerable<Sms> smsCollection = Sms.GetWaitingSmsOnTemplate(_dynamicsCrmConnection, template.Id);

			TextMerger textMerger = new TextMerger(template.new_text);

			report.Workload++;

			List<Dictionary<string, InMobileSms>> smsByNumberList = new List<Dictionary<string, InMobileSms>>();

			foreach (Sms sms in smsCollection)
			{
				report.SubWorkload++;

				string text = GetText(textMerger, template, sms.toid.Value);

				string msisdn = InMobileSms.GetMsisdn(sms.mobilephone);
				InMobileSms inMobileSms = new InMobileSms(text, "", sms.new_sendtime);

				AddToSmsByNumberList(msisdn, inMobileSms);
			}

			report.Success = true;
		}

		private string GetText(TextMerger textMerger, SmsTemplate smsTemplate, Guid contactId)
		{
			bool containsFields = textMerger.ContainsFields();

			if (containsFields)
			{
				IDictionary<string, object> fields = smsTemplate.GetFields(contactId);

				return textMerger.GetMerged(fields, "yyyy-MM-dd HH:mm:ss");
			}
			else
			{
				return smsTemplate.new_text;
			}
		}

		public static List<SendSms> Find(MongoConnection connection)
		{
			List<DatabaseSendSms> options = DatabaseSendSms.ReadAllowed<DatabaseSendSms>(connection);

			return options.Select(option => new SendSms(connection, option)).ToList();
		}
	}
}
