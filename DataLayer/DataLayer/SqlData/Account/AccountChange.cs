using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class AccountChange : AbstractIdData, IModifiedIdData
	{
		public DateTime createdon;
		public DateTime modifiedon { get; set; }

		public string name;

		public string address1_line1;
		public string address1_line2;
		public string address1_city;
		public string address1_postalcode;
		public string emailaddress1;
		public string telephone1;

		public bool new_erindsamlingssted;
		public int new_kkadminmedlemsnr;
		public string new_region;

		public Guid ExternalAccountId { get; private set; }
		public Guid ChangeProviderId { get; private set; }
		public Guid AccountId { get; private set; }

		public Guid? bykoordinatorid;
		public Guid? omraadekoordinatorid;
		public int? kredsellerby;

		private static readonly List<string> _fields = new List<string>()
		{
			"createdon",
			"modifiedon",

			"name",

			"address1_line1",
			"address1_line2",
			"address1_city",
			"address1_postalcode",
			"emailaddress1",
			"telephone1",

			"new_erindsamlingssted",
			"new_kkadminmedlemsnr",
			"new_region",

			"bykoordinatorid",
			"omraadekoordinatorid",
			"kredsellerby",
		};

		private static string _tableName = typeof(AccountChange).Name;

		private SqlConnection _sqlConnection;

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

			string tableName = typeof(AccountChange).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ExternalAccountId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ChangeProviderId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "AccountId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "name", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "createdon", Utilities.DataType.DATETIME, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "modifiedon", Utilities.DataType.DATETIME, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line2", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_city", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_postalcode", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "emailaddress1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "mobilephone", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "telephone1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_erindsamlingssted", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_kkadminmedlemsnr", Utilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_region", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "bykoordinatorid", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "omraadekoordinatorid", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kredsellerby", Utilities.DataType.INT, SqlBoolean.True);

			CreateKeyIfMissing(sqlConnection, _tableName, "AccountId", typeof(Account).Name, "id");
			CreateKeyIfMissing(sqlConnection, _tableName, "bykoordinatorid", typeof(Contact.Contact).Name, "id", false);
			CreateKeyIfMissing(sqlConnection, _tableName, "omraadekoordinatorid", typeof(Contact.Contact).Name, "id", false);

			Utilities.MaintainCompositeForeignKey3Keys(sqlConnection, tableName, "ChangeProviderId", "ExternalAccountId", "AccountId", typeof(ExternalAccount).Name, "ChangeProviderId", "ExternalAccountId", "AccountId");
		}

		public void Insert()
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			AddInsertParameterIfNotNull(name, "name", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(modifiedon, "modifiedon", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(createdon, "createdon", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(address1_line1, "address1_line1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_line2, "address1_line2", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_city, "address1_city", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_postalcode, "address1_postalcode", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(emailaddress1, "emailaddress1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(telephone1, "telephone1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(new_erindsamlingssted, "new_erindsamlingssted", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_kkadminmedlemsnr, "new_kkadminmedlemsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_region, "new_region", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(AccountId, "AccountId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ExternalAccountId, "ExternalAccountId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ChangeProviderId, "ChangeProviderId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(bykoordinatorid, "bykoordinatorid", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(omraadekoordinatorid, "omraadekoordinatorid", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kredsellerby, "kredsellerby", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderColumns);
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("OUTPUT");
			sqlStringBuilder.AppendLine("	Inserted.id");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderParameters);
			sqlStringBuilder.AppendLine(")");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(_sqlConnection, sqlStringBuilder, parameters.ToArray());

			DataRow row = dataTable.Rows[0];
			Id = (Guid)row["id"];
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

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
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
			string tableName = typeof(AccountChange).Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

			sqlStringBuilder.AppendLine("	,AccountId");
			sqlStringBuilder.AppendLine("	,ExternalAccountId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + tableName);
			sqlStringBuilder.AppendLine("WHERE");

			if (idType == IdType.AccountChangeId)
			{
				sqlStringBuilder.AppendLine("	id = @id");
			}
			else if (idType == IdType.AccountId)
			{
				sqlStringBuilder.AppendLine("	AccountId = @id");
			}
			else if (idType == IdType.ExternalAccountId)
			{
				sqlStringBuilder.AppendLine("	ExternalAccountId = @id");
			}
			else if (idType == IdType.ChangeProviderId)
			{
				sqlStringBuilder.AppendLine("	ChangeProviderId = @id");
			}
			else
			{
				throw new ArgumentException($"unknown IdType {idType}");
			}

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			List<AccountChange> accountChanges = new List<AccountChange>();
			foreach (DataRow row in dataTable.Rows)
			{
				AccountChange accountChange = CreateFromRow(sqlConnection, row);

				accountChanges.Add(accountChange);
			}

			return accountChanges;
		}

		private static void AddFieldsToStringBuilder(StringBuilder sqlStringBuilder)
		{
			_fields.ForEach(field => sqlStringBuilder.AppendLine($"	,{field}"));
		}

		private static AccountChange CreateFromRow(SqlConnection sqlConnection, DataRow row)
		{
			Guid externalAccountId = (Guid)row["ExternalAccountId"];
			Guid changeProviderId = (Guid)row["ChangeProviderId"];
			Guid accountId = (Guid)row["AccountId"];

			AccountChange accountChange = new AccountChange(sqlConnection, accountId, externalAccountId, changeProviderId)
			{
				name = ConvertFromDatabaseValue<string>(row["name"]),
				modifiedon = ConvertFromDatabaseValue<DateTime>(row["modifiedon"]),
				createdon = ConvertFromDatabaseValue<DateTime>(row["createdon"]),
				Id = ConvertFromDatabaseValue<Guid>(row["id"]),

				address1_line1 = ConvertFromDatabaseValue<string>(row["address1_line1"]),
				address1_line2 = ConvertFromDatabaseValue<string>(row["address1_line2"]),
				address1_city = ConvertFromDatabaseValue<string>(row["address1_city"]),
				address1_postalcode = ConvertFromDatabaseValue<string>(row["address1_postalcode"]),
				emailaddress1 = ConvertFromDatabaseValue<string>(row["emailaddress1"]),
				telephone1 = ConvertFromDatabaseValue<string>(row["telephone1"]),

				new_erindsamlingssted = ConvertFromDatabaseValue<bool>(row["new_erindsamlingssted"]),
				new_kkadminmedlemsnr = ConvertFromDatabaseValue<int>(row["new_kkadminmedlemsnr"]),
				new_region = ConvertFromDatabaseValue<string>(row["new_region"]),

				bykoordinatorid = ConvertFromDatabaseValue<Guid?>(row["bykoordinatorid"]),
				omraadekoordinatorid = ConvertFromDatabaseValue<Guid?>(row["omraadekoordinatorid"]),
				kredsellerby = ConvertFromDatabaseValue<int?>(row["kredsellerby"]),
			};

			return accountChange;
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

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			DataRow row = dataTable.Rows[0];
			DateTime modifiedOn = (DateTime)row["ModifiedOn"];

			return modifiedOn;
		}
	}
}
