using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Group
{
	public class Group : AbstractIdData
	{
		public string Name;

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(Group).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "Name", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.False);
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(Name, "Name", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine($"	[{TableName}]");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderColumns);
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("OUTPUT");
			sqlStringBuilder.AppendLine("	Inserted.id");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderParameters);
			sqlStringBuilder.AppendLine(")");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());

			DataRow row = dataTable.Rows[0];
			Id = (Guid)row["id"];
		}

		public static Group ReadByName(SqlConnection sqlConnection, string name)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Name");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(Group).Name}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	Name = @name");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("name", name));

			DataRow row = dataTable.Rows[0];
			Group group = CreateFromRow(row);

			return group;
		}

		public static bool ExistsByName(SqlConnection sqlConnection, string name)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT NULL FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(Group).Name}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	Name = @name");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("name", name));

			return dataTable.Rows.Count > 0;
		}

		public static Group ReadNextById(SqlConnection sqlConnection, Guid id)
		{
			return Utilities.ReadNextById<Group>(sqlConnection, id, new string[] { "Name" }, CreateFromRow);
		}

		public static List<Group> ReadGroupsFromContact(SqlConnection sqlConnection, Guid contactId)
		{
			return ReadGroups(sqlConnection, contactId, "ContactId", typeof(ContactGroup));
		}

		public static List<Group> ReadGroupsFromAccount(SqlConnection sqlConnection, Guid accountId)
		{
			return ReadGroups(sqlConnection, accountId, "AccountId", typeof(AccountGroup));
		}

		public static List<Group> ReadGroupsFromContactChange(SqlConnection sqlConnection, Guid contactChangeId)
		{
			return ReadGroups(sqlConnection, contactChangeId, "ContactChangeId", typeof(ContactChangeGroup));
		}

		public static List<Group> ReadGroupsFromAccountChange(SqlConnection sqlConnection, Guid accountChangeId)
		{
			return ReadGroups(sqlConnection, accountChangeId, "AccountChangeId", typeof(AccountChangeGroup));
		}

		private static List<Group> ReadGroups(SqlConnection sqlConnection, Guid foreignKeyId, string foreignKeyName, Type NNTable)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Name");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(Group).Name}]");
			sqlStringBuilder.AppendLine("JOIN");
			sqlStringBuilder.AppendLine($"	{NNTable.Name}");
			sqlStringBuilder.AppendLine("ON");
			sqlStringBuilder.AppendLine($"	{NNTable.Name}.GroupId = [Group].id");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{NNTable.Name}.{foreignKeyName} = @{foreignKeyName}");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(foreignKeyName, foreignKeyId));

			List<Group> groups = ReadGroupsFromDataTable(dataTable);

			return groups;
		}

		private static List<Group> ReadGroupsFromDataTable(DataTable dataTable)
		{
			List<Group> groups = new List<Group>();

			foreach (DataRow row in dataTable.Rows)
			{
				Group group = CreateFromRow(row);

				groups.Add(group);
			}

			return groups;
		}

		private static Group CreateFromRow(DataRow row)
		{
			return new Group
			{
				Id = ConvertFromDatabaseValue<Guid>(row["id"]),
				Name = ConvertFromDatabaseValue<string>(row["Name"]),
			};
		}
	}
}
