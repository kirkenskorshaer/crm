using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Logic
{
	public class Subscriber
	{
		IMailrelayConnection _mailrelayConnection;

		public Subscriber(IMailrelayConnection mailrelayConnection)
		{
			_mailrelayConnection = mailrelayConnection;
		}

		public UpdateResultEnum UpdateIfNeeded(int id, string fullname, string email, Dictionary<string, string> customFields)
		{
			return UpdateIfNeeded(id, fullname, email, customFields, null);
		}

		public UpdateResultEnum UpdateIfNeeded(int id, string fullname, string email, Dictionary<string, string> customFields, int? groupInCrm)
		{
			getSubscribersReply subscriberInMailrelay = GetMailrelaySubscribers(id);

			if (subscriberInMailrelay == null)
			{
				return UpdateResultEnum.Failed;
			}

			Dictionary<string, string> customFieldsResult = new Dictionary<string, string>();
			bool customFieldsChanged = FindNewCustomFields(subscriberInMailrelay, customFields, customFieldsResult);

			List<int> groups = subscriberInMailrelay.groups.Select(group => int.Parse(group)).ToList();

			bool groupsChanged = false;
			if (groupInCrm.HasValue && groups.Contains(groupInCrm.Value) == false)
			{
				groups.Add(groupInCrm.Value);
				groupsChanged = true;
			}

			if
			(
				customFieldsChanged == true ||
				groupsChanged == true ||
				fullname != subscriberInMailrelay.name ||
				email != subscriberInMailrelay.email
			)
			{
				UpdateSubscriber(id, email, customFieldsResult, groups, fullname);
				return UpdateResultEnum.Updated;
			}

			return UpdateResultEnum.AlreadyUpToDate;
		}

		private bool FindNewCustomFields(getSubscribersReply subscriberInMailrelay, Dictionary<string, string> customFieldsInCrm, Dictionary<string, string> customFieldsResult)
		{
			Dictionary<string, string> customFieldsInSubscriberInMailrelay = subscriberInMailrelay.fields;

			List<string> allKeys = customFieldsInCrm.Keys.Union(customFieldsInSubscriberInMailrelay.Keys).ToList();

			bool isChanged = false;
			foreach (string key in allKeys)
			{
				if (customFieldsInSubscriberInMailrelay.ContainsKey(key))
				{
					customFieldsResult.Add(key, customFieldsInSubscriberInMailrelay[key]);
					continue;
				}
				isChanged = true;
				customFieldsResult.Add(key, customFieldsInCrm[key]);
			}

			return isChanged;
		}

		private getSubscribersReply GetMailrelaySubscribers(int id)
		{
			getSubscribers getSubscribers = new getSubscribers()
			{
				id = id,
			};

			MailrelayArrayReply<getSubscribersReply> reply = (MailrelayArrayReply<getSubscribersReply>)_mailrelayConnection.Send(getSubscribers);

			return reply.data.FirstOrDefault();
		}

		private void UpdateSubscriber(int id, string email, Dictionary<string, string> customFieldsResult, List<int> groups, string fullname)
		{
			updateSubscriber update = new updateSubscriber()
			{
				customFields = customFieldsResult,
				email = email,
				name = fullname,
				groups = groups,
				id = id,
			};

			AbstractMailrelayReply reply = _mailrelayConnection.Send(update);
		}
	}
}
