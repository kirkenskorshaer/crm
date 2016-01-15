using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class ExternalAccount : AbstractData
	{
		public Guid ExternalAccountId { get; private set; }
		public Guid ChangeProviderId { get; private set; }

		private SqlConnection _sqlConnection;

		public ExternalAccount(SqlConnection sqlConnection, Guid externalAccountId, Guid changeProviderId)
		{
			ExternalAccountId = externalAccountId;
			ChangeProviderId = changeProviderId;

			_sqlConnection = sqlConnection;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			ChangeProvider.MaintainTable(sqlConnection);

			string tableName = typeof(ExternalAccount).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateCompositeTable2Tables(sqlConnection, tableName, "ChangeProviderId", "ExternalAccountId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "ChangeProviderId", typeof(ChangeProvider).Name, "id");
		}

		public static ExternalAccount Read(SqlConnection sqlConnection, Guid externalAccountId, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	ExternalAccountId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalAccount).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalAccountId = @ExternalAccountId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ExternalAccountId", externalAccountId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			DataRow row = dataTable.Rows[0];
			ExternalAccount externalAccount = CreateFromDataRow(sqlConnection, row);

			return externalAccount;
		}

		public static List<ExternalAccount> Read(SqlConnection sqlConnection, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	ExternalAccountId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalAccount).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			List<ExternalAccount> externalAccounts = new List<ExternalAccount>();
			foreach (DataRow row in dataTable.Rows)
			{
				ExternalAccount externalAccount = CreateFromDataRow(sqlConnection, row);
				externalAccounts.Add(externalAccount);
			}

			return externalAccounts;
		}

		public static List<ExternalAccount> ReadFromChangeProviderAndAccount(SqlConnection sqlConnection, Guid changeProviderId, Guid accountId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT DISTINCT");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalAccount).Name}.ExternalAccountId");
			sqlStringBuilder.AppendLine($"	,{typeof(ExternalAccount).Name}.ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalAccount).Name);
			sqlStringBuilder.AppendLine("JOIN");
			sqlStringBuilder.AppendLine("	" + typeof(AccountChange).Name);
			sqlStringBuilder.AppendLine("ON");
			sqlStringBuilder.AppendLine("	ExternalAccount.ExternalAccountId = AccountChange.ExternalAccountId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ExternalAccount.ChangeProviderId = AccountChange.ChangeProviderId");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalAccount.ChangeProviderId = @ChangeProviderId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	AccountChange.AccountId = @AccountId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("AccountId", accountId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			List<ExternalAccount> externalAccounts = new List<ExternalAccount>();
			foreach (DataRow row in dataTable.Rows)
			{
				ExternalAccount externalAccount = CreateFromDataRow(sqlConnection, row);
				externalAccounts.Add(externalAccount);
			}

			return externalAccounts;
		}

		public static ExternalAccount ReadOrCreate(SqlConnection sqlConnection, Guid externalAccountId, Guid changeProviderId)
		{
			bool externalAccountExists = Exists(sqlConnection, externalAccountId, changeProviderId);

			ExternalAccount externalAccount;

			if (externalAccountExists)
			{
				externalAccount = Read(sqlConnection, externalAccountId, changeProviderId);
			}
			else
			{
				externalAccount = new ExternalAccount(sqlConnection, externalAccountId, changeProviderId);
				externalAccount.Insert();
			}

			return externalAccount;
		}

		public static bool Exists(SqlConnection sqlConnection, Guid externalAccountId, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	NULL");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalAccount).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalAccountId = @ExternalAccountId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ExternalAccountId", externalAccountId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			bool exists = dataTable.Rows.Count == 1;

			return exists;
		}

		private static ExternalAccount CreateFromDataRow(SqlConnection sqlConnection, DataRow row)
		{
			Guid externalAccountId = (Guid)row["ExternalAccountId"];
			Guid changeProviderId = (Guid)row["ChangeProviderId"];

			ExternalAccount externalAccount = new ExternalAccount(sqlConnection, externalAccountId, changeProviderId);

			return externalAccount;
		}

		public void Insert()
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	ExternalAccountId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	@ExternalAccountId");
			sqlStringBuilder.AppendLine("	,@ChangeProviderId");
			sqlStringBuilder.AppendLine(")");

			Utilities.ExecuteNonQuery(_sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("ExternalAccountId", ExternalAccountId)
				, new KeyValuePair<string, object>("ChangeProviderId", ChangeProviderId));
		}
	}
}
