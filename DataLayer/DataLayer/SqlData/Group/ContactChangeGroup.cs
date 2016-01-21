using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Group
{
	public class ContactChangeGroup : AbstractData
	{
		public Guid ContactChangeId { get; private set; }
		public Guid GroupId { get; private set; }

		public ContactChangeGroup(Guid contactChangeId, Guid groupId)
		{
			ContactChangeId = contactChangeId;
			GroupId = groupId;
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
			AddInsertParameterIfNotNull(ContactChangeId, "ContactChangeId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
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
			sqlStringBuilder.AppendLine("	ContactChangeId = @contactChangeId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	GroupId = @groupId");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("contactChangeId", ContactChangeId),
				new KeyValuePair<string, object>("groupId", GroupId));
		}

		public static List<ContactChangeGroup> ReadFromGroupId(SqlConnection sqlConnection, Guid groupId)
		{
			List<Guid> relatedIds = Utilities.ReadNNTable(sqlConnection, typeof(ContactChangeGroup), "GroupId", "ContactChangeId", groupId);

			List<ContactChangeGroup> contactChangeGroups = relatedIds.Select(contactChangeId => new ContactChangeGroup(contactChangeId, groupId)).ToList();

			return contactChangeGroups;
		}

		public static List<ContactChangeGroup> ReadFromContactChangeId(SqlConnection sqlConnection, Guid contactChangeId)
		{
			List<Guid> relatedIds = Utilities.ReadNNTable(sqlConnection, typeof(ContactChangeGroup), "ContactChangeId", "GroupId", contactChangeId);

			List<ContactChangeGroup> contactChangeGroups = relatedIds.Select(groupId => new ContactChangeGroup(contactChangeId, groupId)).ToList();

			return contactChangeGroups;
		}

		public override bool Equals(object obj)
		{
			ContactChangeGroup objAsContactChangeGroup = obj as ContactChangeGroup;

			if(objAsContactChangeGroup == null)
			{
				return false;
			}

			return
				ContactChangeId == objAsContactChangeGroup.ContactChangeId &&
				GroupId == objAsContactChangeGroup.GroupId;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
