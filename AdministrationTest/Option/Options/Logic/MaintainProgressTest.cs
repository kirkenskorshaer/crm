using Administration.Option.Options.Logic;
using NUnit.Framework;
using System;
using DatabaseContact = DataLayer.SqlData.Contact.Contact;
using System.Data.SqlClient;
using DatabaseMaintainProgress = DataLayer.MongoData.Option.Options.Logic.MaintainProgress;
using DatabaseProgress = DataLayer.MongoData.Progress;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class MaintainProgressTest : TestBase
	{
		private SqlConnection _sqlConnection;

		private DatabaseContact _contact;

		[SetUp]
		new public void SetUp()
		{
			base.SetUp();

			_sqlConnection = DataLayer.SqlConnectionHolder.GetConnection(Connection, "sql");

			_contact = new DatabaseContact()
			{
				Firstname = "test",
				Lastname = "test",
				ModifiedOn = DateTime.Now,
				CreatedOn = DateTime.Now,
			};

			_contact.Insert(_sqlConnection);
		}

		[TearDown]
		public void TearDown()
		{
			_contact.Delete(_sqlConnection);
		}

		[Test]
		public void ExecuteOptionCreatesMaintainProgressOnMaintainIfItDoesNotExist()
		{
			DatabaseMaintainProgress databaseMaintainProgress = new DatabaseMaintainProgress()
			{
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			MaintainProgress maintainProgress = new MaintainProgress(Connection, databaseMaintainProgress);

			maintainProgress.Execute();

			DatabaseProgress databaseProgress = DatabaseProgress.ReadNext(Connection, MaintainProgress.MaintainProgressContact);

			Assert.IsNotNull(databaseProgress);
		}

		[Test]
		public void ExecuteOptionCreatesMaintainProgressOnContactIfItDoesNotExist()
		{
			DatabaseMaintainProgress databaseMaintainProgress = new DatabaseMaintainProgress()
			{
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			MaintainProgress maintainProgress = new MaintainProgress(Connection, databaseMaintainProgress);

			maintainProgress.Execute();

			DatabaseProgress databaseProgress = DatabaseProgress.ReadNext(Connection, MaintainProgress.ProgressContact);

			Assert.AreEqual(databaseProgress.TargetId, _contact.Id);
		}

		[Test]
		public void ExecuteOptionRemovesMaintainProgressIfContactDoesNotExist()
		{
			DatabaseMaintainProgress databaseMaintainProgress = new DatabaseMaintainProgress()
			{
				Name = "test",
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			MaintainProgress maintainProgress = new MaintainProgress(Connection, databaseMaintainProgress);

			maintainProgress.Execute();
			_contact.Delete(_sqlConnection);
			maintainProgress.Execute();

			bool databaseProgressExists = DatabaseProgress.Exists(Connection, MaintainProgress.ProgressContact, _contact.Id);

			Assert.IsFalse(databaseProgressExists);
		}
	}
}
