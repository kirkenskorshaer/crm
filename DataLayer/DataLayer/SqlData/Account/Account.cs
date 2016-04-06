using DataLayer.MongoData;
using DataLayer.SqlData.Group;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Account
{
	public class Account : AbstractIdData, IModifiedIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.DATETIME, false)]
		public DateTime createdon;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.DATETIME, false)]
		public DateTime modifiedon { get; set; }

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string name;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string address1_line1;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string address1_line2;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string address1_city;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string address1_postalcode;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string emailaddress1;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, true)]
		public string telephone1;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.INT, true)]
		public int? erindsamlingssted;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.INT, true)]
		public int? new_kkadminmedlemsnr;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.INT, true)]
		public int? region;
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.INT, true)]
		public int? stedtype;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "bykoordinator", typeof(Contact.Contact), "id", false, 1)]
		public Guid? bykoordinatorid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "omraadekoordinator", typeof(Contact.Contact), "id", false, 1)]
		public Guid? omraadekoordinatorid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "korshaersleder", typeof(Contact.Contact), "id", false, 1)]
		public Guid? korshaerslederid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "genbrugskonsulent", typeof(Contact.Contact), "id", false, 1)]
		public Guid? genbrugskonsulentid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "primarycontact", typeof(Contact.Contact), "id", false, 1)]
		public Guid? primarycontact;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "byarbejde", typeof(Byarbejde.Byarbejde), "id", false, 1)]
		public Guid? byarbejdeid;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.INT, true)]
		public int? kredsellerby;

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type tableType = typeof(Account);

			SqlUtilities.MaintainTable(sqlConnection, tableType);
		}

		public void Insert(SqlConnection sqlConnection, MongoConnection mongoConnection)
		{
			Insert(sqlConnection);

			CreateProgressForAccount(mongoConnection);
		}

		private void CreateProgressForAccount(MongoConnection mongoConnection)
		{
			if (mongoConnection == null)
			{
				return;
			}

			string progressName = "Account";

			if (Progress.Exists(mongoConnection, progressName, Id))
			{
				return;
			}

			Progress newAccountProgress = new Progress()
			{
				LastProgressDate = DateTime.Now,
				TargetId = Id,
				TargetName = progressName,
			};

			newAccountProgress.Insert(mongoConnection);
		}

		public static Account ReadNextById(SqlConnection sqlConnection, Guid id)
		{
			return ReadNextById<Account>(sqlConnection, id);
		}

		public static bool Exists(SqlConnection sqlConnection, Guid id)
		{
			return Exists<Account>(sqlConnection, "id", id);
		}

		public static Account Read(SqlConnection sqlConnection, Guid accountId)
		{
			List<Account> accounts = Read<Account>(sqlConnection, "id", accountId);

			return accounts.Single();
		}

		public static List<Account> ReadLatest(SqlConnection sqlConnection, DateTime lastSearchDate)
		{
			List<Account> accounts = Read<Account>(sqlConnection, new SqlCondition("modifiedon", ">=", lastSearchDate), null, null);

			return accounts;
		}

		public void SynchronizeGroups(SqlConnection sqlConnection, List<Guid> groupIds)
		{
			List<AccountGroup> accountGroups = AccountGroup.ReadFromAccountId(sqlConnection, Id);

			foreach (AccountGroup accountGroup in accountGroups)
			{
				if (groupIds.Contains(accountGroup.GroupId) == false)
				{
					accountGroup.Delete(sqlConnection);
				}
			}

			foreach (Guid groupId in groupIds)
			{
				if (accountGroups.Any(accountGroup => accountGroup.GroupId == groupId) == false)
				{
					AccountGroup accountGroup = new AccountGroup(Id, groupId);
					accountGroup.Insert(sqlConnection);
				}
			}
		}

		public void SynchronizeContacts(SqlConnection sqlConnection, List<Guid> contactIds)
		{
			List<AccountContact> accountContacts = AccountContact.ReadFromAccountId(sqlConnection, Id);

			foreach (AccountContact accountContact in accountContacts)
			{
				if (contactIds.Contains(accountContact.ContactId) == false)
				{
					accountContact.Delete(sqlConnection);
				}
			}

			foreach (Guid contactId in contactIds)
			{
				if (accountContacts.Any(accountContact => accountContact.ContactId == contactId) == false)
				{
					AccountContact accountContact = new AccountContact(Id, contactId);
					accountContact.Insert(sqlConnection);
				}
			}
		}

		public void SynchronizeIndsamlere(SqlConnection sqlConnection, List<Guid> contactIds)
		{
			List<AccountIndsamler> accountIndsamlere = AccountIndsamler.ReadFromAccountId(sqlConnection, Id);

			foreach (AccountIndsamler accountIndsamler in accountIndsamlere)
			{
				if (contactIds.Contains(accountIndsamler.ContactId) == false)
				{
					accountIndsamler.Delete(sqlConnection);
				}
			}

			foreach (Guid contactId in contactIds)
			{
				if (accountIndsamlere.Any(accountContact => accountContact.ContactId == contactId) == false)
				{
					AccountIndsamler accountGroup = new AccountIndsamler(Id, contactId);
					accountGroup.Insert(sqlConnection);
				}
			}
		}
	}
}
