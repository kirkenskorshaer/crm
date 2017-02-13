using DataLayer;
using DataLayer.MongoData;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using SystemInterface.Mailrelay.Logic;
using DatabaseUpdateMailrelayFromContact = DataLayer.MongoData.Option.Options.Logic.UpdateMailrelayFromContact;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;

namespace Administration.Option.Options.Logic
{
	public class UpdateMailrelayFromContact : OptionBase
	{
		private DatabaseUpdateMailrelayFromContact _databaseUpdateMailrelayFromContact;

		public UpdateMailrelayFromContact(MongoConnection connection, DataLayer.MongoData.Option.OptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseUpdateMailrelayFromContact = (DatabaseUpdateMailrelayFromContact)databaseOption;
		}

		public override void ExecuteOption(OptionReport report)
		{
			string urlLoginName = _databaseUpdateMailrelayFromContact.urlLoginName;
			int pageSize = _databaseUpdateMailrelayFromContact.pageSize;
			Guid? contactId = _databaseUpdateMailrelayFromContact.contactId;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			PagingInformation pagingInformation = new PagingInformation();

			List<MailrelayInformation> mailrelayInformations = new List<MailrelayInformation>();

			UpdateReport<int> intReport = new UpdateReport<int>();

			while (pagingInformation.FirstRun || pagingInformation.MoreRecords)
			{
				mailrelayInformations = MailrelayInformation.GetMailrelayFromContact(dynamicsCrmConnection, Config.GetResourcePath, pagingInformation, pageSize, contactId);
				mailrelayInformations.ForEach(information => UpdateIfNeeded(dynamicsCrmConnection, information, intReport));
			}

			Log.Write(Connection, intReport.AsLogText("UpdateMailrelayFromContact"), DataLayer.MongoData.Config.LogLevelEnum.OptionReport);

			report.Success = true;
			return;
		}

		private void UpdateIfNeeded(DynamicsCrmConnection dynamicsCrmConnection, MailrelayInformation information, UpdateReport<int> report)
		{
			bool needsUpdate = information.RecalculateContactCheck();
			int id = information.new_mailrelaysubscriberid.Value;

			if (needsUpdate == false)
			{
				report.CollectUpdate(UpdateResultEnum.AlreadyUpToDate, id);
				return;
			}

			information.UpdateContactMailrelaycheck(dynamicsCrmConnection);

			Subscriber subscriber = new Subscriber(_mailrelayConnection);
			UpdateResultEnum result = subscriber.UpdateIfNeeded(id, information.fullname, information.emailaddress1, information.GetCustomFields());
			report.CollectUpdate(result, id);
		}
	}
}
