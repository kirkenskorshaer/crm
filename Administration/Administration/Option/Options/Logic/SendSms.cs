using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Inmobile;
using Utilities;
using DatabaseSendSms = DataLayer.MongoData.Option.Options.Logic.SendSms;

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

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			SmsTemplate template = SmsTemplate.GetWaitingTemplate(_dynamicsCrmConnection);

			if (template == null)
			{
				report.Success = true;
				return;
			}

			SetInMobileConnectionIfEmpty();
			IEnumerable<Sms> smsCollection = Sms.GetWaitingSmsOnTemplate(_dynamicsCrmConnection, template.Id);
			//List<Sms> smsList = smsCollection.ToList();

			TextMerger textMerger = new TextMerger(template.new_text);

			report.Workload++;

			List<Dictionary<string, InMobileSms>> smsByNumberList = new List<Dictionary<string, InMobileSms>>();

			foreach (Sms sms in smsCollection)
			{
				report.SubWorkload++;

				string text = GetText(textMerger, template, sms.contactid.Value);

				string msisdn = InMobileSms.GetMsisdn(sms.mobilephone);
				InMobileSms inMobileSms = new InMobileSms(text, "Kirkens Korshær", sms.new_operatorsendtime ?? sms.new_sendtime);

				inMobileSms.LocalSms = sms;

				AddToSmsByNumberList(msisdn, inMobileSms, smsByNumberList);
			}

			smsByNumberList.ForEach(_inMobileConnection.Send);

			smsByNumberList.ForEach(inMobileSms =>
				inMobileSms.ToList().ForEach(smsByNumber =>
					((Sms)smsByNumber.Value.LocalSms).MarkAsSent(smsByNumber.Value.Text, smsByNumber.Value.MessageId)));

			report.Success = true;
		}

		private void AddToSmsByNumberList(string msisdn, InMobileSms inMobileSms, List<Dictionary<string, InMobileSms>> smsByNumberList)
		{
			foreach (Dictionary<string, InMobileSms> msisdnDictionary in smsByNumberList)
			{
				if (msisdnDictionary.ContainsKey(msisdn) == false)
				{
					msisdnDictionary.Add(msisdn, inMobileSms);
					return;
				}
			}

			Dictionary<string, InMobileSms> msisdnDictionaryCreated = new Dictionary<string, InMobileSms>();

			msisdnDictionaryCreated.Add(msisdn, inMobileSms);

			smsByNumberList.Add(msisdnDictionaryCreated);
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
