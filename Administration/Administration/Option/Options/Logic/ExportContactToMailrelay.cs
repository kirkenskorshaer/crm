using DataLayer;
using DataLayer.MongoData;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.Logic;
using DatabaseExportContactToMailrelay = DataLayer.MongoData.Option.Options.Logic.ExportContactToMailrelay;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;

namespace Administration.Option.Options.Logic
{
	public class ExportContactToMailrelay : OptionBase
	{
		private DatabaseExportContactToMailrelay _databaseExportContactToMailrelay;

		public ExportContactToMailrelay(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseExportContactToMailrelay = (DatabaseExportContactToMailrelay)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseExportContactToMailrelay.urlLoginName;

			Guid? crmListId = _databaseExportContactToMailrelay.listId;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			PagingInformation pagingInformation = new PagingInformation();

			while (pagingInformation.FirstRun || pagingInformation.MoreRecords)
			{
				MarketingList marketingList = MarketingList.GetListForMailrelayUpdate(dynamicsCrmConnection, pagingInformation, crmListId);

				if (marketingList != null)
				{
					UpdateReport<Guid> result = ExportMailrelayContactFromList(dynamicsCrmConnection, marketingList);
					Log.Write(Connection, result.AsLogText($"ExportContactToMailrelay {marketingList.new_mailrelaygroupid.Value}"), DataLayer.MongoData.Config.LogLevelEnum.OptionReport);
				}
			}

			report.Success = true;

			return;
		}

		private UpdateReport<Guid> ExportMailrelayContactFromList(DynamicsCrmConnection dynamicsCrmConnection, MarketingList marketingList)
		{
			if (marketingList.new_mailrelaygroupid.HasValue == false)
			{
				marketingList.new_mailrelaygroupid = Subscriber.CreateGroup(_mailrelayConnection, $"crm_{marketingList.listname}", "crm kontrolleret gruppe");

				marketingList.UpdateMailrelaygroupid(dynamicsCrmConnection);
			}

			IEnumerable<Guid> contactIdsToUpdate = marketingList.ContentIdsForNonMailrelaySubscribers;

			UpdateReport<Guid> updateReport = new UpdateReport<Guid>();
			foreach (Guid contactId in contactIdsToUpdate)
			{
				MailrelayInformation mailrelayInformation = MailrelayInformation.GetInformationNotInMailrelayFromContact(dynamicsCrmConnection, Config.GetResourcePath, contactId);

				UpdateResultEnum result = AddToMailrelay(dynamicsCrmConnection, mailrelayInformation, marketingList.new_mailrelaygroupid.Value);

				updateReport.CollectUpdate(result, contactId);
			}

			return null;
		}

		private UpdateResultEnum AddToMailrelay(DynamicsCrmConnection dynamicsCrmConnection, MailrelayInformation mailrelayInformation, int groupId)
		{
			if (mailrelayInformation.new_mailrelaysubscriberid != null)
			{
				return UpdateResultEnum.AlreadyUpToDate;
			}

			Subscriber subscriber = new Subscriber(_mailrelayConnection);

			try
			{
				getSubscribersReply reply = subscriber.GetMailrelaySubscribers(mailrelayInformation.emailaddress1);

				if (reply == null)
				{
					mailrelayInformation.new_mailrelaysubscriberid = subscriber.AddNewSubscriber(mailrelayInformation.fullname, mailrelayInformation.emailaddress1, new List<int>() { groupId }, mailrelayInformation.GetCustomFields());
				}
				else
				{
					reply.groups.Add(groupId.ToString());

					subscriber.UpdateFromReply(reply);

					mailrelayInformation.new_mailrelaysubscriberid = int.Parse(reply.id);
				}
			}
			catch (Exception exception)
			{
				Log.Write(Connection, exception.Message, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				return UpdateResultEnum.Failed;
			}

			mailrelayInformation.UpdateContactMailrelaySubscriberid(dynamicsCrmConnection);

			return UpdateResultEnum.Updated;
		}
	}
}
