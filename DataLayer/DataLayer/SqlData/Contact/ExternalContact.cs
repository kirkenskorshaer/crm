using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Contact
{
	public class ExternalContact : AbstractData
	{
		public Guid ExternalContactId { get; private set; }
		public Guid ChangeProviderId { get; private set; }

		private SqlConnection _sqlConnection;

		public ExternalContact(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId)
		{
			ExternalContactId = externalContactId;
			ChangeProviderId = changeProviderId;

			_sqlConnection = sqlConnection;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			ChangeProvider.MaintainTable(sqlConnection);

			string tableName = typeof(ExternalContact).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateCompositeTable2Tables(sqlConnection, tableName, "ChangeProviderId", "ExternalContactId");
			}

			CreateKeyIfMissing(sqlConnection, tableName, "ChangeProviderId", typeof(ChangeProvider).Name, "id");
		}

		public static ExternalContact Read(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	ExternalContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalContactId = @ExternalContactId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
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
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
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
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	ExternalContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ExternalContact).Name);
			sqlStringBuilder.AppendLine("JOIN");
			sqlStringBuilder.AppendLine("	" + typeof(ContactChange).Name);
			sqlStringBuilder.AppendLine("ON");
			sqlStringBuilder.AppendLine("	ExternalContact.ExternalContactId = ContactChange.ExternalContactId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ExternalContact.ChangeProviderId = ContactChange.ChangeProviderId");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ExternalContact.ChangeProviderId = @ChangeProviderId");
			sqlStringBuilder.AppendLine("	AND");
			sqlStringBuilder.AppendLine("	ContactChange.ContactId = @ContactId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
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

		public static ExternalContact ReadOrCreate(SqlConnection sqlConnection, Guid externalContactId, Guid changeProviderId)
		{
			bool externalContactExists = Exists(sqlConnection, externalContactId, changeProviderId);

			ExternalContact externalContact;

			if (externalContactExists)
			{
				externalContact = Read(sqlConnection, externalContactId, changeProviderId);
			}
			else
			{
				externalContact = new ExternalContact(sqlConnection, externalContactId, changeProviderId);
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

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ExternalContactId", externalContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			bool exists = dataTable.Rows.Count == 1;

			return exists;
		}

		private static ExternalContact CreateFromDataRow(SqlConnection sqlConnection, DataRow row)
		{
			Guid externalContactId = (Guid)row["ExternalContactId"];
			Guid changeProviderId = (Guid)row["ChangeProviderId"];

			ExternalContact externalContact = new ExternalContact(sqlConnection, externalContactId, changeProviderId);

			return externalContact;
		}

		public void Insert()
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	ExternalContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	@ExternalContactId");
			sqlStringBuilder.AppendLine("	,@ChangeProviderId");
			sqlStringBuilder.AppendLine(")");

			Utilities.ExecuteNonQuery(_sqlConnection, sqlStringBuilder, CommandType.Text,
				new KeyValuePair<string, object>("ExternalContactId", ExternalContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", ChangeProviderId));
		}
	}
}
