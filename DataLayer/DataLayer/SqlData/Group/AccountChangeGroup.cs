using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Group
{
	public class AccountChangeGroup : AbstractData
	{
		public Guid AccountChangeId { get; private set; }
		public Guid GroupId { get; private set; }

		public AccountChangeGroup(Guid accountChangeId, Guid groupId)
		{
			AccountChangeId = accountChangeId;
			GroupId = groupId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(AccountChangeGroup).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateCompositeTable2Tables(sqlConnection, tableName, "AccountChangeId", "GroupId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "AccountChangeId", typeof(Account.AccountChange).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "GroupId", typeof(Group).Name, "id");
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(AccountChangeId, "AccountChangeId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(GroupId, "GroupId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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
			sqlStringBuilder.AppendLine("	GroupId = @groupId");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("accountChangeId", AccountChangeId),
				new KeyValuePair<string, object>("groupId", GroupId));
		}

		public static List<AccountChangeGroup> ReadFromAccountChangeId(SqlConnection sqlConnection, Guid accountChangeId)
		{
			List<Guid> relatedIds = Utilities.ReadNNTable(sqlConnection, typeof(AccountChangeGroup), "AccountChangeId", "GroupId", accountChangeId);

			List<AccountChangeGroup> contactChangeGroups = relatedIds.Select(groupId => new AccountChangeGroup(accountChangeId, groupId)).ToList();

			return contactChangeGroups;
		}

		public static List<AccountChangeGroup> ReadFromGroupId(SqlConnection sqlConnection, Guid groupId)
		{
			List<Guid> relatedIds = Utilities.ReadNNTable(sqlConnection, typeof(AccountChangeGroup), "GroupId", "AccountChangeId", groupId);

			List<AccountChangeGroup> contactChangeGroups = relatedIds.Select(accountChangeId => new AccountChangeGroup(accountChangeId, groupId)).ToList();

			return contactChangeGroups;
		}

		public override bool Equals(object obj)
		{
			AccountChangeGroup objAsAccountChangeGroup = obj as AccountChangeGroup;

			if (objAsAccountChangeGroup == null)
			{
				return false;
			}

			return
				AccountChangeId == objAsAccountChangeGroup.AccountChangeId &&
				GroupId == objAsAccountChangeGroup.GroupId;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
