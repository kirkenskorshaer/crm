using DataLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.GeneralFunctions;
using Utilities;
using DatabaseSendTableFromMailrelay = DataLayer.MongoData.Option.Options.Logic.SendTableFromMailrelay;

namespace Administration.Option.Options.Logic
{
	public class SendTableFromMailrelay : OptionBase
	{
		private DatabaseSendTableFromMailrelay _databaseSendTableFromMailrelay;
		private CultureInfo cultureInfo = new CultureInfo("da-DK");

		public SendTableFromMailrelay(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseSendTableFromMailrelay = (DatabaseSendTableFromMailrelay)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseSendTableFromMailrelay.urlLoginName;
			Guid? contactid = _databaseSendTableFromMailrelay.contactid;
			string contactIdName = _databaseSendTableFromMailrelay.contactidName;
			string queryFindContacts = _databaseSendTableFromMailrelay.queryFindContacts;
			string queryCreateTable = _databaseSendTableFromMailrelay.queryCreateTable;
			string subject = _databaseSendTableFromMailrelay.subject;
			string html = _databaseSendTableFromMailrelay.html;
			string tablerow = _databaseSendTableFromMailrelay.tablerow;
			string matchidname = _databaseSendTableFromMailrelay.matchidname;
			string limitOnDateName = _databaseSendTableFromMailrelay.limitOnDateName;
			List<int> requireDataOnDaysFromToday = _databaseSendTableFromMailrelay.requireDataOnDaysFromToday;
			TimeSpan sleepTimeOnFailiure = _databaseSendTableFromMailrelay.sleepTimeOnFailiure;
			string orderbyDescending = _databaseSendTableFromMailrelay.orderbyDescending;
			string orderby = _databaseSendTableFromMailrelay.orderby;
			string headerDateFormat = _databaseSendTableFromMailrelay.headerDateFormat;
			string tableDateFormat = _databaseSendTableFromMailrelay.tableDateFormat;

			int mailboxfromid = _databaseSendTableFromMailrelay.mailboxfromid;
			int mailboxreplyid = _databaseSendTableFromMailrelay.mailboxreplyid;
			int mailboxreportid = _databaseSendTableFromMailrelay.mailboxreportid;
			int packageid = _databaseSendTableFromMailrelay.packageid;

			string smtpHost = _databaseSendTableFromMailrelay.smtpHost;
			string fromEmail = _databaseSendTableFromMailrelay.fromEmail;
			string toEmail = _databaseSendTableFromMailrelay.toEmail;
			int port = _databaseSendTableFromMailrelay.port;
			string smtpUsername = _databaseSendTableFromMailrelay.smtpUsername;
			string smtpPassword = _databaseSendTableFromMailrelay.smtpPassword;

			DatabaseSendTableFromMailrelay.SendTypeEnum sendType = _databaseSendTableFromMailrelay.sendType;

			SetDynamicsCrmConnectionIfEmpty(urlLoginName);

			PagingInformation pagingInformation = new PagingInformation();

			XDocument xDocument = XDocument.Parse(queryFindContacts);
			XmlHelper.SetCount(xDocument, 1);
			/*
			if (contactid.HasValue)
			{
				XmlHelper.AddCondition(xDocument, contactIdName, "eq", contactid.ToString());
			}
			*/
			if (string.IsNullOrWhiteSpace(headerDateFormat))
			{
				headerDateFormat = "yyyy-MM-dd";
			}

			if (string.IsNullOrWhiteSpace(tableDateFormat))
			{
				tableDateFormat = "yyyy-MM-dd";
			}

			while (pagingInformation.FirstRun || pagingInformation.MoreRecords)
			{
				dynamic receiver = DynamicFetch.ReadFromFetchXml(_dynamicsCrmConnection, xDocument, pagingInformation).SingleOrDefault();

				if (receiver == null)
				{
					report.Success = true;
					return;
				}

				IDictionary<string, object> receiverDictionary = (IDictionary<string, object>)receiver;

				if (contactid.HasValue)
				{
					if ((Guid)receiverDictionary[contactIdName] != contactid)
					{
						continue;
					}
				}

				if (receiverDictionary.ContainsKey("emailaddress1") == false)
				{
					continue;
				}

				report.Workload++;

				string fullname = receiver.fullname;
				string emailaddress1 = receiver.emailaddress1;
				Guid matchid = receiver.matchid;

				if (string.IsNullOrWhiteSpace(toEmail) == false)
				{
					emailaddress1 = toEmail;
				}

				XDocument tableDocument = XDocument.Parse(queryCreateTable);

				XmlHelper.AddCondition(tableDocument, matchidname, "eq", matchid.ToString());

				List<dynamic> tableRowList = DynamicFetch.ReadFromFetchXml(_dynamicsCrmConnection, tableDocument, new PagingInformation());

				StringBuilder rowBuilder = new StringBuilder();

				if (string.IsNullOrWhiteSpace(orderby) == false)
				{
					tableRowList = tableRowList.OrderBy(row =>
					{
						IDictionary<string, object> rowDictionary = (IDictionary<string, object>)row;
						object sortObject = null;
						if (rowDictionary.ContainsKey(orderby))
						{
							sortObject = rowDictionary[orderby];
						}
						return sortObject;
					}).ToList();
				}
				if (string.IsNullOrWhiteSpace(orderbyDescending) == false)
				{
					tableRowList = tableRowList.OrderByDescending(row =>
					{
						IDictionary<string, object> rowDictionary = (IDictionary<string, object>)row;
						object sortObject = null;
						if (rowDictionary.ContainsKey(orderbyDescending))
						{
							sortObject = rowDictionary[orderbyDescending];
						}
						return sortObject;
					}).ToList();
				}

				foreach (dynamic rowDynamic in tableRowList)
				{
					IDictionary<string, object> rowDictionary = (IDictionary<string, object>)rowDynamic;

					string row = InsertFilledFields(tablerow, rowDictionary, tableDateFormat);

					rowBuilder.Append(row);
				}

				string htmlWithRows = html.Replace("{rows}", rowBuilder.ToString());

				receiver.RowCount = tableRowList.Count();
				DateTime today = DateTime.Now.Date;
				receiver.Today = today;
				receiver.Yesterday = today.AddDays(-1);

				if (string.IsNullOrWhiteSpace(limitOnDateName) == false)
				{
					if (requireDataOnDaysFromToday == null || requireDataOnDaysFromToday.Any() == false)
					{
						continue;
					}

					bool isMailAllowed = false;
					foreach (int daysToAdd in requireDataOnDaysFromToday)
					{
						int rowCount = FindRowCountInPeriod(tableRowList, limitOnDateName, today.AddDays(daysToAdd), today.AddDays(daysToAdd + 1));

						receiverDictionary["RowCountDay" + daysToAdd.ToString()] = rowCount;

						if (rowCount != 0)
						{
							isMailAllowed = true;
						}
					}

					if (isMailAllowed == false)
					{
						continue;
					}
				}

				report.SubWorkload += receiver.RowCount;

				string htmlWithRowsAndFields = InsertFilledFields(htmlWithRows, receiverDictionary, headerDateFormat);

				bool mailSent = false;

				if (sendType == DatabaseSendTableFromMailrelay.SendTypeEnum.Api)
				{
					mailSent = TrySendMail(htmlWithRowsAndFields, fullname, emailaddress1, subject, packageid, mailboxfromid, mailboxreplyid, mailboxreportid);
				}

				if (sendType == DatabaseSendTableFromMailrelay.SendTypeEnum.Smtp)
				{
					mailSent = TrySendSmtpMail(htmlWithRowsAndFields, fullname, fromEmail, emailaddress1, subject, smtpHost, port, smtpUsername, smtpPassword);
				}

				if (mailSent == false)
				{
					Log.Write(Connection, $"failed sending email to {emailaddress1} sleeping {sleepTimeOnFailiure}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
					Thread.Sleep(sleepTimeOnFailiure);
				}

				report.TextBuilder.AppendLine($"Sent to {emailaddress1} - Rows {receiver.RowCount}");
			}

			report.Success = true;
		}

		private bool TrySendSmtpMail(string htmlWithRowsAndFields, string fullname, string fromEmail, string emailaddress1, string subject, string smtpHost, int port, string smtpUsername, string smtpPassword)
		{
			SystemInterface.Email emailSender = new SystemInterface.Email();

			try
			{
				emailSender.Send(htmlWithRowsAndFields, true, subject, fromEmail, emailaddress1, smtpHost, port, smtpUsername, smtpPassword);
				return true;
			}
			catch (Exception exception)
			{
				Log.Write(Connection, exception.Message, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				return false;
			}
		}

		private int FindRowCountInPeriod(List<dynamic> rowDynamicList, string limitOnDateName, DateTime from, DateTime to)
		{
			int rowCount = 0;

			foreach (dynamic rowDynamic in rowDynamicList)
			{
				IDictionary<string, object> rowDictionary = (IDictionary<string, object>)rowDynamic;

				if (rowDictionary.ContainsKey(limitOnDateName))
				{
					DateTime rowTime = (DateTime)rowDictionary[limitOnDateName];

					if (DateTimeHelper.Between(rowTime, from, to))
					{
						rowCount++;
					}
				}
			}

			return rowCount;
		}

		private bool TrySendMail(string html, string fullName, string email, string subject, int packageId, int mailboxFromId, int mailboxReplyId, int mailboxReportId)
		{
			sendMail sendMailFunction = new sendMail()
			{
				html = html,
				emails = new Dictionary<string, string>() { { fullName, email } },
				subject = subject,
				packageId = packageId,
				mailboxFromId = mailboxFromId,
				mailboxReplyId = mailboxReplyId,
				mailboxReportId = mailboxReportId,
			};

			try
			{
				_mailrelayConnection.Send(sendMailFunction);
				return true;
			}
			catch (Exception exception)
			{
				Log.Write(Connection, exception.Message, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				return false;
			}
		}

		public string InsertFilledFields(string originalString, IDictionary<string, object> fieldDictionary, string dateFormat)
		{
			MatchCollection matches = Regex.Matches(originalString, "\\{![^\\}]*\\}");

			foreach (Match match in matches)
			{
				string matchedString = match.Value;
				matchedString = matchedString.Substring(2, matchedString.Length - 3);
				string[] options = matchedString.Split(';');

				for (int optionIndex = 0; optionIndex < options.Length - 1; optionIndex++)
				{
					string option = options[optionIndex];
					if (fieldDictionary.ContainsKey(option))
					{
						object value = fieldDictionary[option];
						string stringValue;

						if (value is DateTime)
						{
							stringValue = ((DateTime)value).ToString(dateFormat, cultureInfo);
						}
						else
						{
							stringValue = value.ToString();
						}

						originalString = originalString.Replace(match.Value, stringValue);
					}
				}

				originalString = originalString.Replace(match.Value, options.Last());
			}

			return originalString;
		}
	}
}
