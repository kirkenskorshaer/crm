using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Group
{
	public class ContactGroup : AbstractData
	{
		public Guid ContactId { get; private set; }
		public Guid GroupId { get; private set; }

		public ContactGroup(Guid contactId, Guid groupId)
		{
			ContactId = contactId;
			GroupId = groupId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(ContactGroup).Name;

			List<string> columnsInDatabase = SqlUtilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				SqlUtilities.CreateCompositeTable2Tables(sqlConnection, tableName, "ContactId", "GroupId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "ContactId", typeof(Contact.Contact).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "GroupId", typeof(Group).Name, "id");
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(ContactId, "ContactId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(GroupId, "GroupId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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
			sqlStringBuilder.AppendLine("	ContactId = @contactId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	GroupId = @groupId");

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("contactId", ContactId),
				new KeyValuePair<string, object>("groupId", GroupId));
		}

		public static List<ContactGroup> ReadFromContactId(SqlConnection sqlConnection, Guid contactId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(ContactGroup), "ContactId", "GroupId", contactId);

			List<ContactGroup> contactGroups = relatedIds.Select(groupId => new ContactGroup(contactId, groupId)).ToList();

			return contactGroups;
		}

		public static List<ContactGroup> ReadFromGroupId(SqlConnection sqlConnection, Guid groupId)
		{
			List<Guid> relatedIds = SqlUtilities.ReadNNTable(sqlConnection, typeof(ContactGroup), "GroupId", "ContactId", groupId);

			List<ContactGroup> contactGroups = relatedIds.Select(contactId => new ContactGroup(contactId, groupId)).ToList();

			return contactGroups;
		}
	}
}
