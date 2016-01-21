using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class AccountChangeContact : AbstractData
	{
		public Guid AccountChangeId { get; private set; }
		public Guid ContactId { get; private set; }

		public AccountChangeContact(Guid accountChangeId, Guid contactId)
		{
			AccountChangeId = accountChangeId;
			ContactId = contactId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(AccountChangeContact).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateCompositeTable2Tables(sqlConnection, tableName, "AccountChangeId", "ContactId");
			}

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

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
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

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("accountChangeId", AccountChangeId),
				new KeyValuePair<string, object>("contactId", ContactId));
		}

		public static List<AccountChangeContact> ReadFromAccountChangeId(SqlConnection sqlConnection, Guid accountChangeId)
		{
			List<Guid> relatedIds = Utilities.ReadNNTable(sqlConnection, typeof(AccountChangeContact), "AccountChangeId", "ContactId", accountChangeId);

			List<AccountChangeContact> accountChangeIndsamlere = relatedIds.Select(contactId => new AccountChangeContact(accountChangeId, contactId)).ToList();

			return accountChangeIndsamlere;
		}

		public static List<AccountChangeContact> ReadFromContactId(SqlConnection sqlConnection, Guid contactId)
		{
			List<Guid> relatedIds = Utilities.ReadNNTable(sqlConnection, typeof(AccountChangeContact), "ContactId", "AccountChangeId", contactId);

			List<AccountChangeContact> accountChangeIndsamlere = relatedIds.Select(accountChangeId => new AccountChangeContact(accountChangeId, contactId)).ToList();

			return accountChangeIndsamlere;
		}
	}
}
