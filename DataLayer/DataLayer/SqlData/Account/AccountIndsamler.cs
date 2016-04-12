using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class AccountIndsamler : AbstractData
	{
		public Guid AccountId { get; private set; }
		public Guid ContactId { get; private set; }

		public AccountIndsamler()
		{
		}

		public AccountIndsamler(Guid AccountId, Guid ContactId)
		{
			this.AccountId = AccountId;
			this.ContactId = ContactId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(AccountIndsamler).Name;

			List<string> columnsInDatabase = SqlUtilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				SqlUtilities.CreateCompositeTable2Tables(sqlConnection, tableName, "AccountId", "ContactId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "AccountId", typeof(Account).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "ContactId", typeof(Contact.Contact).Name, "id");
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(AccountId, "AccountId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ContactId, "ContactId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine($"	[{TableName}]");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderColumns);
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderParameters);
			sqlStringBuilder.AppendLine(")");

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
		}

		public void Delete(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("DELETE FROM");
			sqlStringBuilder.AppendLine($"	[{TableName}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	AccountId = @AccountId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ContactId = @contactId");

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("AccountId", AccountId),
				new KeyValuePair<string, object>("contactId", ContactId));
		}

		public static List<AccountIndsamler> ReadFromContactId(SqlConnection sqlConnection, Guid contactId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(AccountIndsamler), "ContactId", "AccountId", contactId);

			List<AccountIndsamler> accountIndsamlere = relatedIds.Select(AccountId => new AccountIndsamler(AccountId, contactId)).ToList();

			return accountIndsamlere;
		}

		public static List<AccountIndsamler> ReadFromAccountId(SqlConnection sqlConnection, Guid accountId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(AccountIndsamler), "AccountId", "ContactId", accountId);

			List<AccountIndsamler> accountIndsamlere = relatedIds.Select(ContactId => new AccountIndsamler(accountId, ContactId)).ToList();

			return accountIndsamlere;
		}

		public static AccountIndsamler ReadIfExists(SqlConnection sqlConnection, Guid accountId, Guid contactId)
		{
			AccountIndsamler accountIndsamler = Read<AccountIndsamler>(sqlConnection, new List<SqlCondition>()
			{
				new SqlCondition("AccountId", "=", accountId),
				new SqlCondition("ContactId", "=", contactId)
			}).FirstOrDefault();

			return accountIndsamler;
		}
	}
}
