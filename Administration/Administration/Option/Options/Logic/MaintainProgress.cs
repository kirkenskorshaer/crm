using Administration.Option.Options.Data;
using DataLayer;
using System;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseMaintainProgress = DataLayer.MongoData.Option.Options.Logic.MaintainProgress;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseProgress = DataLayer.MongoData.Progress;

namespace Administration.Option.Options.Logic
{
	public class MaintainProgress : AbstractDataOptionBase
	{
		public const string MaintainProgressContact = "MaintainProgressContact";
		public const string ProgressContact = "Contact";
		public const string ProgressContactToCrm = "ContactToCrm";

		private DatabaseMaintainProgress _databaseMaintainProgress;

		public MaintainProgress(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseMaintainProgress = (DatabaseMaintainProgress)databaseOption;
		}

		protected override bool ExecuteOption()
		{
			DatabaseProgress progress = DatabaseProgress.ReadNext(Connection, MaintainProgressContact);

			if (progress == null)
			{
				progress = new DatabaseProgress()
				{
					LastProgressDate = DateTime.Now,
					TargetId = Guid.Empty,
					TargetName = MaintainProgressContact,
				};

				progress.Insert(Connection);
			}

			Guid currentId = progress.TargetId;

			RemoveLastContactProgressIfContactIsDeleted(currentId);

			DatabaseContact contact = DatabaseContact.ReadNextById(SqlConnection, currentId);

			if (contact == null)
			{
				Log.Write(Connection, "No contact to maintain", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
				return false;
			}

			if (DatabaseProgress.Exists(Connection, ProgressContact, contact.Id) == false)
			{
				DatabaseProgress newContactProgress = new DatabaseProgress()
				{
					LastProgressDate = DateTime.Now,
					TargetId = contact.Id,
					TargetName = ProgressContact,
				};

				newContactProgress.Insert(Connection);
			}

			progress.TargetId = contact.Id;
			progress.UpdateAndSetLastProgressDateToNow(Connection);

			return true;
		}

		private void RemoveLastContactProgressIfContactIsDeleted(Guid currentId)
		{
			bool contactExists = DatabaseContact.Exists(SqlConnection, currentId);

			if (contactExists)
			{
				return;
			}

			if (DatabaseProgress.Exists(Connection, ProgressContact, currentId))
			{
				DatabaseProgress contactProgress = DatabaseProgress.Read(Connection, ProgressContact, currentId);

				contactProgress.Delete(Connection);
			}
		}
	}
}
