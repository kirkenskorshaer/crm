using System;
using System.Collections.Generic;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SendTableFromMailrelay : OptionBase
	{
		public string urlLoginName { get; set; }

		public Guid? contactid { get; set; }
		public string contactidName { get; set; }
		public string queryFindContacts { get; set; }
		public string queryCreateTable { get; set; }
		public string subject { get; set; }
		public string html { get; set; }
		public string tablerow { get; set; }
		public string matchidname { get; set; }
		public string limitOnDateName { get; set; }
		public List<int> requireDataOnDaysFromToday { get; set; }
		public TimeSpan sleepTimeOnFailiure { get; set; }
		public string orderbyDescending { get; set; }
		public string orderby { get; set; }
		public string headerDateFormat { get; set; }
		public string tableDateFormat { get; set; }

		public int mailboxfromid { get; set; }
		public int mailboxreplyid { get; set; }
		public int mailboxreportid { get; set; }
		public int packageid { get; set; }

		public string smtpHost { get; set; }
		public string fromEmail { get; set; }
		public string toEmail { get; set; }
		public int port { get; set; }
		public string smtpUsername { get; set; }
		public string smtpPassword { get; set; }

		public SendTypeEnum sendType { get; set; }

		public enum SendTypeEnum
		{
			Api = 1,
			Smtp = 2
		}

		public static SendTableFromMailrelay Create
		(
			MongoConnection mongoConnection,
			string name,
			Schedule schedule,

			Guid? contactid,
			string urlLoginName,
			string queryFindContacts,
			string queryCreateTable,
			string subject,
			string html,
			string tablerow,
			string matchidname,
			string limitOnDateName,
			List<int> requireDataOnDaysFromToday,
			TimeSpan sleepTimeOnFailiure,
			string orderbyDescending,
			string headerDateFormat,
			string tableDateFormat,

			int mailboxfromid,
			int mailboxreplyid,
			int mailboxreportid,
			int packageid
		)
		{
			SendTableFromMailrelay sendMailFromMailrelay = new SendTableFromMailrelay()
			{
				urlLoginName = urlLoginName,
				contactid = contactid,
				queryFindContacts = queryFindContacts,
				queryCreateTable = queryCreateTable,
				subject = subject,
				html = html,
				tablerow = tablerow,
				matchidname = matchidname,
				limitOnDateName = limitOnDateName,
				requireDataOnDaysFromToday = requireDataOnDaysFromToday,
				sleepTimeOnFailiure = sleepTimeOnFailiure,
				orderbyDescending = orderbyDescending,
				headerDateFormat = headerDateFormat,
				tableDateFormat = tableDateFormat,

				mailboxfromid = mailboxfromid,
				mailboxreplyid = mailboxreplyid,
				mailboxreportid = mailboxreportid,
				packageid = packageid,
			};

			Create(mongoConnection, sendMailFromMailrelay, name, schedule);

			return sendMailFromMailrelay;
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SendTableFromMailrelay>(connection);
			}
			else
			{
				Delete<SendTableFromMailrelay>(connection);
			}
		}
	}
}
