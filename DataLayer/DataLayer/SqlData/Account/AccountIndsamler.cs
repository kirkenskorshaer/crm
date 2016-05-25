using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class AccountIndsamler : AbstractData
	{
		public Guid AccountId { get; private set; }
		public Guid ContactId { get; private set; }
		public IndsamlerTypeEnum indsamlertype { get; private set; }
		public int aar { get; private set; }

		public AccountIndsamler()
		{
		}

		public AccountIndsamler(Guid AccountId, Guid ContactId, IndsamlerTypeEnum indsamlerType, int aar)
		{
			this.AccountId = AccountId;
			this.ContactId = ContactId;
			indsamlertype = indsamlerType;
			this.aar = aar;
		}

		public enum IndsamlerTypeEnum
		{
			Indsamlingshjaelper = 1,
			Indsamler = 2,
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(AccountIndsamler).Name;

			List<string> columnsInDatabase = SqlUtilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				SqlUtilities.CreateCompositeTable2Tables(sqlConnection, tableName, "AccountId", "ContactId");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "indsamlertype", SqlUtilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "aar", SqlUtilities.DataType.INT, SqlBoolean.True);

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
			AddInsertParameterIfNotNull(aar, "aar", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull((int)indsamlertype, "indsamlertype", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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

			List<AccountIndsamler> accountIndsamlere = relatedIds.Select(AccountId => Read(sqlConnection, AccountId, contactId)).ToList();

			return accountIndsamlere;
		}

		public static List<AccountIndsamler> ReadFromAccountId(SqlConnection sqlConnection, Guid accountId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(AccountIndsamler), "AccountId", "ContactId", accountId);

			List<AccountIndsamler> accountIndsamlere = relatedIds.Select(ContactId => Read(sqlConnection, accountId, ContactId)).ToList();

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

		public static AccountIndsamler Read(SqlConnection sqlConnection, Guid accountId, Guid contactId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine($"	[{nameof(AccountIndsamler)}].aar");
			sqlStringBuilder.AppendLine($"	,[{nameof(AccountIndsamler)}].indsamlertype");
			sqlStringBuilder.AppendLine($"	,[{nameof(AccountIndsamler)}].AccountId");
			sqlStringBuilder.AppendLine($"	,[{nameof(AccountIndsamler)}].ContactId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{nameof(AccountIndsamler)}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{nameof(AccountIndsamler)}.AccountId = @accountId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine($"	{nameof(AccountIndsamler)}.ContactId = @contactId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("AccountId", accountId)
				, new KeyValuePair<string, object>("ContactId", contactId));

			DataRow row = dataTable.Rows[0];
			AccountIndsamler accountIndsamler = CreateFromRow(row);

			return accountIndsamler;
		}

		private static AccountIndsamler CreateFromRow(DataRow row)
		{
			return new AccountIndsamler
			(
				ConvertFromDatabaseValue<Guid>(row["AccountId"]),
				ConvertFromDatabaseValue<Guid>(row["ContactId"]),
				(AccountIndsamler.IndsamlerTypeEnum)ConvertFromDatabaseValue<int>(row["indsamlertype"]),
				ConvertFromDatabaseValue<int>(row["aar"])
			);
		}
	}
}
