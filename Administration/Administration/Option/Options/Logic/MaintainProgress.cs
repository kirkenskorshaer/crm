﻿using Administration.Option.Options.Data;
using DataLayer;
using System;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseMaintainProgress = DataLayer.MongoData.Option.Options.Logic.MaintainProgress;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using DatabaseProgress = DataLayer.MongoData.Progress;
using System.Collections.Generic;
using System.Linq;

namespace Administration.Option.Options.Logic
{
	public class MaintainProgress : AbstractDataOptionBase
	{
		public const string MaintainProgressContact = "MaintainProgressContact";
		public const string ProgressContact = "Contact";
		public const string ProgressAccount = "Account";
		public const string ProgressContactToCrm = "ContactToCrm";
		public const string ProgressAccountToCrm = "AccountToCrm";

		private DatabaseMaintainProgress _databaseMaintainProgress;

		public MaintainProgress(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseMaintainProgress = (DatabaseMaintainProgress)databaseOption;
		}

		public static List<MaintainProgress> Find(MongoConnection connection)
		{
			List<DatabaseMaintainProgress> options = DatabaseOptionBase.ReadAllowed<DatabaseMaintainProgress>(connection);

			return options.Select(option => new MaintainProgress(connection, option)).ToList();
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
