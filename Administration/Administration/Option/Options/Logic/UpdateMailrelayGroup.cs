using DataLayer;
using DataLayer.MongoData;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using SystemInterface.Mailrelay.Logic;
using DatabaseUpdateMailrelayGroup = DataLayer.MongoData.Option.Options.Logic.UpdateMailrelayGroup;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;

namespace Administration.Option.Options.Logic
{
	public class UpdateMailrelayGroup : OptionBase
	{
		private DatabaseUpdateMailrelayGroup _databaseUpdateMailrelayGroup;

		public UpdateMailrelayGroup(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseUpdateMailrelayGroup = (DatabaseUpdateMailrelayGroup)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseUpdateMailrelayGroup.urlLoginName;

			Guid? crmListId = _databaseUpdateMailrelayGroup.listId;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			PagingInformation pagingInformation = new PagingInformation();

			while (pagingInformation.FirstRun || pagingInformation.MoreRecords)
			{
				MarketingList marketingList = MarketingList.GetListForMailrelayUpdate(dynamicsCrmConnection, pagingInformation, crmListId);

				if (marketingList != null)
				{
					UpdateReport<int> result = UpdateMailrelayGroupFromList(dynamicsCrmConnection, marketingList);
					Log.Write(Connection, result.AsLogText($"UpdateMailrelayGroup {marketingList.new_mailrelaygroupid.Value}"), typeof(UpdateMailrelayGroup), DataLayer.MongoData.Config.LogLevelEnum.OptionReport);
				}
			}

			report.Success = true;
			return;
		}

		private UpdateReport<int> UpdateMailrelayGroupFromList(DynamicsCrmConnection dynamicsCrmConnection, MarketingList marketingList)
		{
			int? id = marketingList.new_mailrelaygroupid;

			if (id.HasValue == false)
			{
				id = Subscriber.CreateGroup(_mailrelayConnection, $"crm_{marketingList.listname}", "crm kontrolleret gruppe");

				marketingList.new_mailrelaygroupid = id;

				marketingList.UpdateMailrelaygroupid(dynamicsCrmConnection);
			}

			bool needsUpdate = marketingList.RecalculateMarketingListCheck();

			if (needsUpdate == false)
			{
				return new UpdateReport<int>();
			}

			UpdateReport<int> report = AddOrRemoveMembersFromMailrelayGroup(dynamicsCrmConnection, marketingList, id.Value);

			marketingList.UpdateMailrelaycheck(dynamicsCrmConnection);

			return report;
		}

		private UpdateReport<int> AddOrRemoveMembersFromMailrelayGroup(DynamicsCrmConnection dynamicsCrmConnection, MarketingList marketingList, int groupId)
		{
			List<KeyValueEntity<Guid, int?>> crmIdsAndSubscriberIds = marketingList.CrmIdsAndSubscriberIds;

			List<getSubscribersReply> subscribersToRemoveFromMailrelay = new List<getSubscribersReply>();

			Subscriber subscriber = new Subscriber(_mailrelayConnection);

			getSubscribers getSubscribersFunction = new getSubscribers()
			{
				groups = new List<int>() { groupId },
			};

			IEnumerable<getSubscribersReply> subscribersInMailrelay = subscriber.GetMailrelaySubscribers(getSubscribersFunction, 100);

			foreach (getSubscribersReply currentInMailrelay in subscribersInMailrelay)
			{
				int removed = crmIdsAndSubscriberIds.RemoveAll(idInCrm => currentInMailrelay.id == idInCrm.value.ToString());

				if (removed == 0)
				{
					subscribersToRemoveFromMailrelay.Add(currentInMailrelay);
				}
			}

			UpdateReport<int> report = new UpdateReport<int>();

			RemoveFromMailrelayGroup(report, subscribersToRemoveFromMailrelay, subscriber, groupId);
			AddToMailrelayGroup(dynamicsCrmConnection, report, subscriber, crmIdsAndSubscriberIds, groupId);

			return report;
		}

		private UpdateReport<int> RemoveFromMailrelayGroup(UpdateReport<int> report, List<getSubscribersReply> subscribersToRemoveFromMailrelay, Subscriber subscriber, int groupInCrm)
		{
			foreach (getSubscribersReply reply in subscribersToRemoveFromMailrelay)
			{
				reply.groups.RemoveAll(groupString => groupString == groupInCrm.ToString());

				if (reply.groups.Any() == false)
				{
					reply.groups.Add("1");
				}

				MailrelayBoolReply updateReply = subscriber.UpdateFromReply(reply);

				if (updateReply.status == 0)
				{
					report.CollectUpdate(UpdateResultEnum.Failed, int.Parse(reply.id));
				}
				else
				{
					report.CollectUpdate(UpdateResultEnum.Updated, int.Parse(reply.id));
				}
			}

			return report;
		}

		private void AddToMailrelayGroup(DynamicsCrmConnection dynamicsCrmConnection, UpdateReport<int> report, Subscriber subscriber, List<KeyValueEntity<Guid, int?>> crmIdsAndSubscriberIds, int groupInCrm)
		{
			foreach (KeyValueEntity<Guid, int?> idPair in crmIdsAndSubscriberIds)
			{
				MailrelayInformation mailrelayInformation = MailrelayInformation.GetMailrelayFromContact(dynamicsCrmConnection, Config.GetResourcePath, new PagingInformation(), 1, idPair.key).Single();

				UpdateResultEnum result = subscriber.UpdateIfNeeded(idPair.value.Value, mailrelayInformation.fullname, mailrelayInformation.emailaddress1, mailrelayInformation.GetCustomFields(), groupInCrm);

				report.CollectUpdate(result, idPair.value.Value);
			}
		}
	}
}
