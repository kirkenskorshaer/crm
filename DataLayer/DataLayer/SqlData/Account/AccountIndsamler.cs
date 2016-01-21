using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Contact
{
	public class AccountIndsamler : AbstractData
	{
		public Guid AccountId { get; private set; }
		public Guid ContactId { get; private set; }

		public AccountIndsamler(Guid AccountId, Guid ContactId)
		{
			this.AccountId = AccountId;
			this.ContactId = ContactId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(AccountIndsamler).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateCompositeTable2Tables(sqlConnection, tableName, "AccountId", "ContactId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "AccountId", typeof(Account.Account).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "ContactId", typeof(Contact).Name, "id");
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

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
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

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("AccountId", AccountId),
				new KeyValuePair<string, object>("contactId", ContactId));
		}
	}
}
