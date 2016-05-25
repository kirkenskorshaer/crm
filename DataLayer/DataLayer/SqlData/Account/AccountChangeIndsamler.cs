using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class AccountChangeIndsamler : AbstractData
	{
		public Guid AccountChangeId { get; private set; }
		public Guid ContactId { get; private set; }
		public AccountIndsamler.IndsamlerTypeEnum indsamlertype { get; private set; }
		public int aar { get; private set; }

		public AccountChangeIndsamler(Guid accountChangeId, Guid contactId, AccountIndsamler.IndsamlerTypeEnum indsamlerType, int aar)
		{
			AccountChangeId = accountChangeId;
			ContactId = contactId;
			indsamlertype = indsamlerType;
			this.aar = aar;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(AccountChangeIndsamler).Name;

			List<string> columnsInDatabase = SqlUtilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				SqlUtilities.CreateCompositeTable2Tables(sqlConnection, tableName, "AccountChangeId", "ContactId");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "indsamlertype", SqlUtilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "aar", SqlUtilities.DataType.INT, SqlBoolean.True);

			CreateKeyIfMissing(sqlConnection, tableName, "AccountChangeId", typeof(AccountChange).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "ContactId", typeof(Contact.Contact).Name, "id");
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(AccountChangeId, "AccountChangeId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ContactId, "ContactId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(aar, "aar", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull((int)indsamlertype, "indsamlertype", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
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
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	AccountChangeId = @accountChangeId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ContactId = @contactId");

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("accountChangeId", AccountChangeId),
				new KeyValuePair<string, object>("contactId", ContactId));
		}

		public static List<AccountChangeIndsamler> ReadFromAccountChangeId(SqlConnection sqlConnection, Guid accountChangeId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(AccountChangeIndsamler), "AccountChangeId", "ContactId", accountChangeId);

			List<AccountChangeIndsamler> accountChangeIndsamlere = relatedIds.Select(contactId => Read(sqlConnection, accountChangeId, contactId)).ToList();

			return accountChangeIndsamlere;
		}

		public static List<AccountChangeIndsamler> ReadFromContactId(SqlConnection sqlConnection, Guid contactId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(AccountChangeIndsamler), "ContactId", "AccountChangeId", contactId);

			List<AccountChangeIndsamler> accountChangeIndsamlere = relatedIds.Select(accountChangeId => Read(sqlConnection, accountChangeId, contactId)).ToList();

			return accountChangeIndsamlere;
		}

		public static AccountChangeIndsamler Read(SqlConnection sqlConnection, Guid accountChangeId, Guid contactId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine($"	[{nameof(AccountChangeIndsamler)}].aar");
			sqlStringBuilder.AppendLine($"	,[{nameof(AccountChangeIndsamler)}].indsamlertype");
			sqlStringBuilder.AppendLine($"	,[{nameof(AccountChangeIndsamler)}].AccountChangeId");
			sqlStringBuilder.AppendLine($"	,[{nameof(AccountChangeIndsamler)}].ContactId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{nameof(AccountChangeIndsamler)}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{nameof(AccountChangeIndsamler)}.AccountChangeId = @accountChangeId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine($"	{nameof(AccountChangeIndsamler)}.ContactId = @contactId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("AccountChangeId", accountChangeId)
				, new KeyValuePair<string, object>("ContactId", contactId));

			DataRow row = dataTable.Rows[0];
			AccountChangeIndsamler accountChangeIndsamler = CreateFromRow(row);

			return accountChangeIndsamler;
		}

		private static AccountChangeIndsamler CreateFromRow(DataRow row)
		{
			return new AccountChangeIndsamler
			(
				ConvertFromDatabaseValue<Guid>(row["AccountChangeId"]),
				ConvertFromDatabaseValue<Guid>(row["ContactId"]),
				(AccountIndsamler.IndsamlerTypeEnum)ConvertFromDatabaseValue<int>(row["indsamlertype"]),
				ConvertFromDatabaseValue<int>(row["aar"])
			);
		}

		public override bool Equals(object obj)
		{
			AccountChangeIndsamler objAsAccountChangeIndsamler = obj as AccountChangeIndsamler;

			if (objAsAccountChangeIndsamler == null)
			{
				return false;
			}

			return
				AccountChangeId == objAsAccountChangeIndsamler.AccountChangeId &&
				ContactId == objAsAccountChangeIndsamler.ContactId;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
