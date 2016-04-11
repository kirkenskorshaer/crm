using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class AccountChange : AbstractIdData, IModifiedIdData
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

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "ExternalAccount", typeof(ExternalAccount), "ExternalAccountId", true, 2)]
		public Guid ExternalAccountId { get; private set; }
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "ExternalAccount", typeof(ExternalAccount), "ChangeProviderId", true, 1)]
		public Guid ChangeProviderId { get; private set; }
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, new string[] { "account", "ExternalAccount" }, new Type[] { typeof(Account), typeof(ExternalAccount) }, new string[] { "id", "AccountId" }, new bool[] { true, true }, new int[] { 1, 3 })]
		public Guid AccountId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "bykoordinator", typeof(Contact.Contact), "id", false, 1)]
		public Guid? bykoordinatorid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "omraadekoordinator", typeof(Contact.Contact), "id", false, 1)]
		public Guid? omraadekoordinatorid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "korshaersleder", typeof(Contact.Contact), "id", false, 1)]
		public Guid? korshaerslederid;
		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "genbrugskonsulent", typeof(Contact.Contact), "id", false, 1)]
		public Guid? genbrugskonsulentid;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "indsamlingskoordinator", typeof(Contact.Contact), "id", false, 1)]
		public Guid? indsamlingskoordinatorid;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "primarycontact", typeof(Contact.Contact), "id", false, 1)]
		public Guid? primarycontact;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, true, "byarbejde", typeof(Byarbejde.Byarbejde), "id", false, 1)]
		public Guid? byarbejdeid;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.INT, true)]
		public int? kredsellerby;

		private SqlConnection _sqlConnection;

		public AccountChange()
		{
		}

		public AccountChange(SqlConnection sqlConnection, Guid accountId, Guid externalAccountId, Guid changeProviderId)
		{
			ExternalAccountId = externalAccountId;
			ChangeProviderId = changeProviderId;
			AccountId = accountId;

			_sqlConnection = sqlConnection;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Account.MaintainTable(sqlConnection);
			ExternalAccount.MaintainTable(sqlConnection);

			Type tableType = typeof(AccountChange);

			SqlUtilities.MaintainTable(sqlConnection, tableType);
		}

		public void Insert()
		{
			Insert(_sqlConnection);
		}

		public static bool AccountChangeExists(SqlConnection sqlConnection, Guid accountId, Guid externalAccountId, Guid changeProviderId, DateTime modifiedOn)
		{
			string tableName = typeof(AccountChange).Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT TOP 1");
			sqlStringBuilder.AppendLine("	NULL");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	EXISTS");
			sqlStringBuilder.AppendLine("	(");
			sqlStringBuilder.AppendLine("		SELECT");
			sqlStringBuilder.AppendLine("			*");
			sqlStringBuilder.AppendLine("		FROM");
			sqlStringBuilder.AppendLine("			" + tableName);
			sqlStringBuilder.AppendLine("		WHERE");
			sqlStringBuilder.AppendLine("			AccountChange.AccountId = @AccountId");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			AccountChange.ExternalAccountId = @ExternalAccountId");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			AccountChange.ChangeProviderId = @ChangeProviderId");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			CASE");
			sqlStringBuilder.AppendLine("				WHEN");
			sqlStringBuilder.AppendLine("					DATEDIFF(DAY, ModifiedOn, @ModifiedOn) = 0");
			sqlStringBuilder.AppendLine("				THEN");
			sqlStringBuilder.AppendLine("					CASE");
			sqlStringBuilder.AppendLine("						WHEN");
			sqlStringBuilder.AppendLine("							DATEDIFF(MILLISECOND, ModifiedOn, @ModifiedOn) = 0");
			sqlStringBuilder.AppendLine("						THEN");
			sqlStringBuilder.AppendLine("							0");
			sqlStringBuilder.AppendLine("						ELSE");
			sqlStringBuilder.AppendLine("							1");
			sqlStringBuilder.AppendLine("					END");
			sqlStringBuilder.AppendLine("			END = 0");
			sqlStringBuilder.AppendLine("	)");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("AccountId", accountId)
				, new KeyValuePair<string, object>("ExternalAccountId", externalAccountId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId)
				, new KeyValuePair<string, object>("ModifiedOn", modifiedOn));

			bool exists = dataTable.Rows.Count == 1;

			return exists;
		}

		public enum IdType
		{
			AccountChangeId = 1
			, AccountId = 2
			, ExternalAccountId = 4
			, ChangeProviderId = 8
		}

		public static List<AccountChange> Read(SqlConnection sqlConnection, Guid id, IdType idType)
		{
			List<AccountChange> accountChanges = new List<AccountChange>();
			if (idType == IdType.AccountChangeId)
			{
				accountChanges = Read<AccountChange>(sqlConnection, "id", id);
			}
			else if (idType == IdType.AccountId)
			{
				accountChanges = Read<AccountChange>(sqlConnection, "AccountId", id);
			}
			else if (idType == IdType.ExternalAccountId)
			{
				accountChanges = Read<AccountChange>(sqlConnection, "ExternalAccountId", id);
			}
			else if (idType == IdType.ChangeProviderId)
			{
				accountChanges = Read<AccountChange>(sqlConnection, "ChangeProviderId", id);
			}
			else
			{
				throw new ArgumentException($"unknown IdType {idType}");
			}

			accountChanges.ForEach(accountChange => accountChange._sqlConnection = sqlConnection);

			return accountChanges;
		}

		public static List<Account> GetAccounts(SqlConnection sqlConnection, Guid externalAccountId)
		{
			List<AccountChange> accountChanges = Read(sqlConnection, externalAccountId, IdType.ExternalAccountId);

			List<Guid> accountIds = accountChanges.Select(accountChange => accountChange.AccountId).Distinct().ToList();

			List<Account> accounts = accountChanges.Select(accountChange => Account.Read(sqlConnection, accountChange.AccountId)).ToList();

			return accounts;
		}

		public static List<ExternalAccount> GetExternalAccounts(SqlConnection sqlConnection, Guid accountId, Guid changeProviderId)
		{
			List<AccountChange> accountChanges = Read(sqlConnection, accountId, IdType.AccountId);

			List<Guid> externalAccountIds = accountChanges.Select(accountChange => accountChange.ExternalAccountId).Distinct().ToList();

			List<ExternalAccount> externalAccounts = externalAccountIds.Select(externalAccountId => ExternalAccount.Read(sqlConnection, externalAccountId, changeProviderId)).ToList();

			return externalAccounts;
		}

		public static List<ExternalAccount> GetExternalAccounts(SqlConnection sqlConnection, Guid accountId)
		{
			List<AccountChange> accountChanges = Read(sqlConnection, accountId, IdType.AccountId).Distinct().ToList();

			List<ExternalAccount> externalAccounts = accountChanges.Select(AccountChange => ExternalAccount.Read(sqlConnection, AccountChange.ExternalAccountId, AccountChange.ChangeProviderId)).ToList();

			return externalAccounts;
		}

		public static DateTime GetLatestModifiedOn(SqlConnection sqlConnection, Guid changeProviderId)
		{
			string tableName = typeof(AccountChange).Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	COALESCE(MAX(ModifiedOn),'2000-01-01') ModifiedOn");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + tableName);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	AccountChange.ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			DataRow row = dataTable.Rows[0];
			DateTime modifiedOn = (DateTime)row["ModifiedOn"];

			return modifiedOn;
		}
	}
}
