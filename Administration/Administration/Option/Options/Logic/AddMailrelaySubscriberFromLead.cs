using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Function.Subscribers;
using SystemInterface.Mailrelay.FunctionReply;
using DatabaseAddMailrelaySubscriberFromLead = DataLayer.MongoData.Option.Options.Logic.AddMailrelaySubscriberFromLead;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using DatabaseWebCampaign = DataLayer.MongoData.Input.WebCampaign;

namespace Administration.Option.Options.Logic
{
	public class AddMailrelaySubscriberFromLead : OptionBase
	{
		private DatabaseAddMailrelaySubscriberFromLead _databaseAddMailrelaySubscriberFromLead;

		public AddMailrelaySubscriberFromLead(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseAddMailrelaySubscriberFromLead = (DatabaseAddMailrelaySubscriberFromLead)databaseOption;
		}

		protected override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseAddMailrelaySubscriberFromLead.urlLoginName;
			string email = _databaseAddMailrelaySubscriberFromLead.email;
			Guid leadId = _databaseAddMailrelaySubscriberFromLead.leadId;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			DatabaseWebCampaign webCampaign = DatabaseWebCampaign.ReadByIdBytesSingleOrDefault(Connection, _databaseAddMailrelaySubscriberFromLead.WebCampaignIdValue());

			if (webCampaign == null)
			{
				Log.Write(Connection, $"Could not find campaign for {_databaseAddMailrelaySubscriberFromLead.Name}", DataLayer.MongoData.Config.LogLevelEnum.OptionError);
				report.Success = false;
				return;
			}

			MailrelayInformation information = GetInformationFromFetchXml(dynamicsCrmConnection, webCampaign, email);

			if (information == null)
			{
				Log.Write(Connection, $"Information for lead {leadId} on {email} could not be found", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
				report.Success = true;
				return;
			}

			if (information.campaign_new_mailrelaygroupid.HasValue == false)
			{
				Log.Write(Connection, $"Subscriber not added, campaign {webCampaign.FormId} has no group", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
				report.Success = true;
				return;
			}

			getSubscribersReply ExistingSubscriber = GetExistingSubscribers(email);

			int subscriberId;
			if (ExistingSubscriber == null)
			{
				addSubscriber add = GetSubscriberFromFetchXml(information, email);

				subscriberId = SendSubscriberToMailrelay(add);
			}
			else
			{
				subscriberId = int.Parse(ExistingSubscriber.id);
				UdpateExistingSubscriberIfNeeded(information, ExistingSubscriber, email, subscriberId);
			}

			try
			{
				Lead.UpdateSubscriberId(dynamicsCrmConnection, leadId, subscriberId);
			}
			catch (Exception exception)
			{
				Log.Write(Connection, exception.Message, exception.StackTrace, DataLayer.MongoData.Config.LogLevelEnum.OptionError);
			}

			report.Success = true;
		}

		private void UdpateExistingSubscriberIfNeeded(MailrelayInformation information, getSubscribersReply ExistingSubscriber, string email, int subscriberId)
		{
			Dictionary<string, string> customFieldsResult = new Dictionary<string, string>();

			bool isChanged = FindNewCustomFields(information, ExistingSubscriber, customFieldsResult);

			List<int> groups = ExistingSubscriber.groups.Select(group => int.Parse(group)).ToList();
			if (groups.Contains(information.campaign_new_mailrelaygroupid.Value) == false)
			{
				groups.Add(information.campaign_new_mailrelaygroupid.Value);
				isChanged = true;
			}

			if (information.fullname != ExistingSubscriber.name)
			{
				isChanged = true;
			}

			if (isChanged)
			{
				UpdateSubscriber(email, customFieldsResult, groups, information.fullname, subscriberId);
			}
		}

		private void UpdateSubscriber(string email, Dictionary<string, string> customFieldsResult, List<int> groups, string fullname, int subscriberId)
		{
			updateSubscriber update = new updateSubscriber()
			{
				id = subscriberId,
				customFields = customFieldsResult,
				email = email,
				name = fullname,
				groups = groups,
			};

			AbstractMailrelayReply reply = _mailrelayConnection.Send(update);
		}

		private bool FindNewCustomFields(MailrelayInformation information, getSubscribersReply ExistingSubscriber, Dictionary<string, string> customFieldsResult)
		{
			Dictionary<string, string> customFieldsInInformation = information.GetCustomFields();
			Dictionary<string, string> customFieldsInExistingSubscriber = ExistingSubscriber.fields;

			List<string> allKeys = customFieldsInInformation.Keys.Union(customFieldsInExistingSubscriber.Keys).ToList();

			bool isChanged = false;
			foreach (string key in allKeys)
			{
				if (customFieldsInExistingSubscriber.ContainsKey(key))
				{
					customFieldsResult.Add(key, customFieldsInExistingSubscriber[key]);
					continue;
				}
				isChanged = true;
				customFieldsResult.Add(key, customFieldsInInformation[key]);
			}

			return isChanged;
		}

		private MailrelayInformation GetInformationFromFetchXml(DynamicsCrmConnection dynamicsCrmConnection, DatabaseWebCampaign webCampaign, string email)
		{
			Guid campaignId = webCampaign.FormId;

			MailrelayInformation information = MailrelayInformation.GetMailrelayFromLead(dynamicsCrmConnection, Config.GetResourcePath, email, campaignId);

			return information;
		}

		private getSubscribersReply GetExistingSubscribers(string email)
		{
			getSubscribers getSubscribers = new getSubscribers()
			{
				email = email,
				deleted = false,
			};

			MailrelayArrayReply<getSubscribersReply> reply = (MailrelayArrayReply<getSubscribersReply>)_mailrelayConnection.Send(getSubscribers);

			return reply.data.Where(data => data.email == email).SingleOrDefault();
		}

		private addSubscriber GetSubscriberFromFetchXml(MailrelayInformation information, string email)
		{
			Dictionary<string, string> customFields = information.GetCustomFields();

			List<int> groups = new List<int>() { information.campaign_new_mailrelaygroupid.Value };

			addSubscriber add = new addSubscriber()
			{
				customFields = customFields,
				email = email,
				name = information.fullname,
				groups = groups,
			};

			return add;
		}

		private int SendSubscriberToMailrelay(addSubscriber add)
		{
			MailrelayIntReply intReply = (MailrelayIntReply)_mailrelayConnection.Send(add);

			return intReply.data;
		}

		public static DatabaseAddMailrelaySubscriberFromLead CreateIfValid(MongoConnection connection, Guid leadId, string name, string urlLoginName, string emailaddress1, DatabaseWebCampaign webCampaign)
		{
			if (string.IsNullOrWhiteSpace(emailaddress1))
			{
				return null;
			}

			if (webCampaign == null)
			{
				return null;
			}

			DatabaseAddMailrelaySubscriberFromLead databaseAddMailrelaySubscriberFromLead = DatabaseAddMailrelaySubscriberFromLead.Create
			(
				connection,
				webCampaign,
				urlLoginName,
				leadId,
				emailaddress1,
				name,
				new DataLayer.MongoData.Option.Schedule()
				{
					NextAllowedExecution = DateTime.Now,
					Recurring = false,
					TimeBetweenAllowedExecutions = TimeSpan.FromSeconds(30),
				}
			);

			return databaseAddMailrelaySubscriberFromLead;
		}
	}
}
