﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Group
{
	public class ContactChangeGroup : AbstractData
	{
		private Guid _contactChangeId;
		private Guid _groupId;

		public ContactChangeGroup(Guid ContactChangeId, Guid GroupId)
		{
			_contactChangeId = ContactChangeId;
			_groupId = GroupId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(ContactChangeGroup).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateCompositeTable2Tables(sqlConnection, tableName, "ContactChangeId", "GroupId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "ContactChangeId", typeof(Contact.ContactChange).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "GroupId", typeof(Group).Name, "id");
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(_contactChangeId, "ContactChangeId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(_groupId, "GroupId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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
			sqlStringBuilder.AppendLine("	ContactChangeId = @contactChangeId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	GroupId = @groupId");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("contactChangeId", _contactChangeId),
				new KeyValuePair<string, object>("groupId", _groupId));
		}
	}
}