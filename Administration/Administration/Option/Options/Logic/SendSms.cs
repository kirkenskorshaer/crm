﻿using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
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
			string accountSid = _databaseSendSms.accountSid;
			string authToken = _databaseSendSms.authToken;
			string fromNumber = _databaseSendSms.fromNumber;
			string statusCallback = _databaseSendSms.statusCallback;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			SmsTemplate template = SmsTemplate.GetWaitingTemplate(_dynamicsCrmConnection);

			SetTwilioConnectionIfEmpty(fromNumber, accountSid, authToken, statusCallback);
			IEnumerable<Sms> smsCollection = Sms.GetWaitingSmsOnTemplate(_dynamicsCrmConnection, template.Id);

			TextMerger textMerger = new TextMerger(template.new_text);

			report.Workload++;

			foreach (Sms sms in smsCollection)
			{
				report.SubWorkload++;

				string text = GetText(textMerger, template, sms.toid.Value);

				TwilioMessageInfo messageInfo = _twilioConnection.Send(sms.mobilephone, text);

				sms.MarkAsSent(text, messageInfo.Sid);
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
	}
}
