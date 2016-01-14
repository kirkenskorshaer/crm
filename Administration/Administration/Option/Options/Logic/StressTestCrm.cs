using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using DatabaseStressTestCrm = DataLayer.MongoData.Option.Options.Logic.StressTestCrm;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using SystemInterfaceContact = SystemInterface.Dynamics.Crm.Contact;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using SystemInterface.Dynamics.Crm;
using Administration.Mapping.Contact;
using DataLayer.MongoData.Statistics.StringStatistics;
using System.Linq;

namespace Administration.Option.Options.Logic
{
	public class StressTestCrm : AbstractDataOptionBase
	{
		private DatabaseStressTestCrm _databaseStressTestCrm;
		private DynamicsCrmConnection _dynamicsCrmConnection;

		public StressTestCrm(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseStressTestCrm = (DatabaseStressTestCrm)DatabaseOption;

			string urlLoginName = _databaseStressTestCrm.urlLoginName;
			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			_dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
		}

		public SystemInterfaceContact CreateContact()
		{
			SystemInterfaceContact contact = new SystemInterfaceContact(_dynamicsCrmConnection);

			ContactAnonymousMapping.GetInstance().AnonymizeContact(contact);

			contact.Groups = contact.Groups.Select(group => Group.ReadOrCreate(_dynamicsCrmConnection, group.Name)).ToList();

			contact.Insert();

			return contact;
		}

		public static List<StressTestCrm> Find(MongoConnection connection)
		{
			List<DatabaseStressTestCrm> options = DatabaseOptionBase.ReadAllowed<DatabaseStressTestCrm>(connection);

			return options.Select(option => new StressTestCrm(connection, option)).ToList();
		}

		protected override bool ExecuteOption()
		{
			int contactsToCreate = _databaseStressTestCrm.contactsToCreate;
			DataLayer.MongoData.IntProgress intProgress = GetIntProgress();

			if (intProgress.progressValue == 0)
			{
				DeleteAllContacts();
			}

			DateTime BeforeContacts = DateTime.Now;

			for (int contactsIndex = 0; contactsIndex < contactsToCreate; contactsIndex++)
			{
				CreateContact();
			}

			DateTime AfterContacts = DateTime.Now;

			int milliSecondsForEachContact = (int)((AfterContacts - BeforeContacts).TotalMilliseconds / contactsToCreate);

			int step = 1000;
			int min = (intProgress.progressValue / step) * step;
			int max = min + step;

			StringIntStatistics.Create(Connection, intProgress.TargetName, $"{min} - {max}", milliSecondsForEachContact);

			intProgress.progressValue += contactsToCreate;
			intProgress.Update(Connection);

			return true;
		}

		private DataLayer.MongoData.IntProgress GetIntProgress()
		{
			string targetName = "StressTestCrm";
			Guid targetId = Guid.Empty;

			DataLayer.MongoData.IntProgress intProgress;
			if (DataLayer.MongoData.IntProgress.Exists(Connection, targetName, targetId))
			{
				intProgress = DataLayer.MongoData.IntProgress.Read(Connection, targetName, targetId);
			}
			else
			{
				intProgress = new DataLayer.MongoData.IntProgress()
				{
					progressValue = 0,
					TargetId = targetId,
					TargetName = targetName,
				};

				intProgress.Insert(Connection);
			}

			return intProgress;
		}

		private void DeleteAllContacts()
		{
			List<SystemInterfaceContact> contacts = SystemInterfaceContact.ReadLatest(_dynamicsCrmConnection, DateTime.MinValue);

			Log.Write(Connection, $"starting to delete {contacts.Count} contacts", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			foreach (SystemInterfaceContact contact in contacts)
			{
				contact.Delete();
			}

			Log.Write(Connection, "contacts deleted", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
		}
	}
}
