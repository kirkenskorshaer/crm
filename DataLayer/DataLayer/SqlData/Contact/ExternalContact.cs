using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Contact
{
	public class ExternalContact : AbstractData
	{
		public Guid ExternalContactId { get; private set; }
		public Guid ContactId { get; private set; }
		public Guid ChangeProviderId { get; private set; }

		private SqlConnection _sqlConnection;

		public ExternalContact(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId, Guid contactId)
		{
			ExternalContactId = externalContactId;
			ChangeProviderId = changeProviderId;
			ContactId = contactId;

			_sqlConnection = sqlConnection;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			ChangeProvider.MaintainTable(sqlConnection);

			string tableName = typeof(ExternalContact).Name;

			List<string> columnsInDatabase = SqlUtilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				SqlUtilities.CreateCompositeTable3Tables(sqlConnection, tableName, "ChangeProviderId", "ExternalContactId", "ContactId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "ChangeProviderId", typeof(ChangeProvider).Name, "id");
			CreateKeyIfMissing(sqlConnection, tableName, "ContactId", typeof(Contact).Name, "id", false);

			SqlUtilities.MaintainUniqueConstraint(sqlConnection, tableName, tableName + "_ChangeProvider_ExternalContact", "ChangeProviderId", "ExternalContactId");
		}

		public static ExternalContact Read(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	ExternalContactId");
			sqlStringBuilder.AppendLine("	,ContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalContactId = @ExternalContactId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ExternalContactId", externalContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			DataRow row = dataTable.Rows[0];
			ExternalContact externalContact = CreateFromDataRow(sqlConnection, row);

			return externalContact;
		}

		public static List<ExternalContact> Read(SqlConnection sqlConnection, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	ExternalContactId");
			sqlStringBuilder.AppendLine("	,ContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			List<ExternalContact> externalContacts = new List<ExternalContact>();
			foreach (DataRow row in dataTable.Rows)
			{
				ExternalContact externalContact = CreateFromDataRow(sqlConnection, row);
				externalContacts.Add(externalContact);
			}

			return externalContacts;
		}

		public static List<ExternalContact> ReadFromChangeProviderAndContact(SqlConnection sqlConnection, Guid changeProviderId, Guid contactId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT DISTINCT");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalContact).Name}.ExternalContactId");
			sqlStringBuilder.AppendLine($"	,{typeof(ExternalContact).Name}.ContactId");
			sqlStringBuilder.AppendLine($"	,{typeof(ExternalContact).Name}.ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalContact).Name}.ChangeProviderId = @ChangeProviderId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalContact).Name}.ContactId = @ContactId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ContactId", contactId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			List<ExternalContact> externalContacts = new List<ExternalContact>();
			foreach (DataRow row in dataTable.Rows)
			{
				ExternalContact externalContact = CreateFromDataRow(sqlConnection, row);
				externalContacts.Add(externalContact);
			}

			return externalContacts;
		}

		public static List<ExternalContact> ReadFromChangeProviderAndExternalContact(SqlConnection sqlConnection, Guid changeProviderId, Guid externalContactId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT DISTINCT");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalContact).Name}.ExternalContactId");
			sqlStringBuilder.AppendLine($"	,{typeof(ExternalContact).Name}.ContactId");
			sqlStringBuilder.AppendLine($"	,{typeof(ExternalContact).Name}.ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalContact).Name}.ChangeProviderId = @ChangeProviderId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine($"	{typeof(ExternalContact).Name}.ExternalContactId = @ExternalContactId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ExternalContactId", externalContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			List<ExternalContact> externalContacts = new List<ExternalContact>();
			foreach (DataRow row in dataTable.Rows)
			{
				ExternalContact externalContact = CreateFromDataRow(sqlConnection, row);
				externalContacts.Add(externalContact);
			}

			return externalContacts;
		}

		public static ExternalContact ReadOrCreate(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId, Guid contactId)
		{
			bool externalContactExists = Exists(sqlConnection, externalContactId, changeProviderId);

			ExternalContact externalContact;

			if (externalContactExists)
			{
				externalContact = Read(sqlConnection, externalContactId, changeProviderId);
			}
			else
			{
				externalContact = new ExternalContact(sqlConnection, externalContactId, changeProviderId, contactId);
				externalContact.Insert();
			}

			return externalContact;
		}

		public static bool Exists(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	NULL");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalContactId = @ExternalContactId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ExternalContactId", externalContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			bool exists = dataTable.Rows.Count == 1;

			return exists;
		}

		private static ExternalContact CreateFromDataRow(SqlConnection sqlConnection, DataRow row)
		{
			Guid externalContactId = (Guid)row["ExternalContactId"];
			Guid contactId = (Guid)row["ContactId"];
			Guid changeProviderId = (Guid)row["ChangeProviderId"];

			ExternalContact externalContact = new ExternalContact(sqlConnection, externalContactId, changeProviderId, contactId);

			return externalContact;
		}

		public void Insert()
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	ExternalContactId");
			sqlStringBuilder.AppendLine("	,ContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	@ExternalContactId");
			sqlStringBuilder.AppendLine("	,@ContactId");
			sqlStringBuilder.AppendLine("	,@ChangeProviderId");
			sqlStringBuilder.AppendLine(")");

			SqlUtilities.ExecuteNonQuery(_sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("ExternalContactId", ExternalContactId)
				, new KeyValuePair<string, object>("ContactId", ContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", ChangeProviderId));
		}
	}
}
