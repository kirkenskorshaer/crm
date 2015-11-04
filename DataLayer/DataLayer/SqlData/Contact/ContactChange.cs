using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Contact
{
	public class ContactChange : AbstractIdData
	{
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;

		public Guid ExternalContactId { get; private set; }
		public Guid ChangeProviderId { get; private set; }
		public Guid ContactId { get; private set; }

		private static string _tableName = typeof(ContactChange).Name;

		private SqlConnection _sqlConnection;

		public ContactChange(SqlConnection sqlConnection, Guid contactId, Guid externalContactId, Guid changeProviderId)
		{
			ExternalContactId = externalContactId;
			ChangeProviderId = changeProviderId;
			ContactId = contactId;

			_sqlConnection = sqlConnection;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Contact.MaintainTable(sqlConnection);
			ExternalContact.MaintainTable(sqlConnection);

			string tableName = typeof(ContactChange).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "FirstName", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "LastName", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "CreatedOn", Utilities.DataType.DATETIME, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ModifiedOn", Utilities.DataType.DATETIME, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ExternalContactId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ChangeProviderId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ContactId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			CreateKeyIfMissing(sqlConnection, _tableName, "ContactId", typeof(Contact).Name, "id");

			Utilities.MaintainCompositeForeignKey2Keys(sqlConnection, tableName, "ChangeProviderId", "ExternalContactId", typeof(ExternalContact).Name, "ChangeProviderId", "ExternalContactId");
		}

		public void Insert()
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(Firstname, "Firstname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(Lastname, "Lastname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ModifiedOn, "ModifiedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(CreatedOn, "CreatedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ContactId, "ContactId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ExternalContactId, "ExternalContactId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ChangeProviderId, "ChangeProviderId", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderColumns);
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("OUTPUT");
			sqlStringBuilder.AppendLine("	Inserted.id");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.Append(sqlStringBuilderParameters);
			sqlStringBuilder.AppendLine(")");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(_sqlConnection, sqlStringBuilder, parameters.ToArray());

			DataRow row = dataTable.Rows[0];
			Id = (Guid)row["id"];
		}

		public enum IdType
		{
			ContactChangeId = 1
			, ContactId = 2
			, ExternalContactId = 4
			, ChangeProviderId = 8
		}

		public static List<ContactChange> Read(SqlConnection sqlConnection, Guid id, IdType idType)
		{
			string tableName = typeof(ContactChange).Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Firstname");
			sqlStringBuilder.AppendLine("	,Lastname");
			sqlStringBuilder.AppendLine("	,ModifiedOn");
			sqlStringBuilder.AppendLine("	,CreatedOn");
			sqlStringBuilder.AppendLine("	,ContactId");
			sqlStringBuilder.AppendLine("	,ExternalContactId");
			sqlStringBuilder.AppendLine("	,ChangeProviderId");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + tableName);
			sqlStringBuilder.AppendLine("WHERE");

			if (idType == IdType.ContactChangeId)
			{
				sqlStringBuilder.AppendLine("	id = @id");
			}
			else if (idType == IdType.ContactId)
			{
				sqlStringBuilder.AppendLine("	ContactId = @id");
			}
			else if (idType == IdType.ExternalContactId)
			{
				sqlStringBuilder.AppendLine("	ExternalContactId = @id");
			}
			else if (idType == IdType.ChangeProviderId)
			{
				sqlStringBuilder.AppendLine("	ChangeProviderId = @id");
			}
			else
			{
				throw new ArgumentException($"unknown IdType {idType}");
			}

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			List<ContactChange> contactChanges = new List<ContactChange>();
			foreach (DataRow row in dataTable.Rows)
			{
				ContactChange contactChange = CreateFromRow(sqlConnection, row);

				contactChanges.Add(contactChange);
			}

			return contactChanges;
		}

		private static ContactChange CreateFromRow(SqlConnection sqlConnection, DataRow row)
		{
			Guid externalContactId = (Guid)row["ExternalContactId"];
			Guid changeProviderId = (Guid)row["ChangeProviderId"];
			Guid contactId = (Guid)row["ContactId"];

			ContactChange contactChange = new ContactChange(sqlConnection, contactId, externalContactId, changeProviderId)
			{
				Firstname = ConvertFromDatabaseValue<string>(row["Firstname"]),
				Lastname = ConvertFromDatabaseValue<string>(row["LastName"]),
				ModifiedOn = ConvertFromDatabaseValue<DateTime>(row["ModifiedOn"]),
				CreatedOn = ConvertFromDatabaseValue<DateTime>(row["CreatedOn"]),
				Id = ConvertFromDatabaseValue<Guid>(row["id"]),
			};

			return contactChange;
		}

		public static List<Contact> GetContacts(SqlConnection sqlConnection, Guid externalContactId)
		{
			List<ContactChange> contactChanges = Read(sqlConnection, externalContactId, IdType.ExternalContactId);

			List<Guid> contacsIds = contactChanges.Select(contactChange => contactChange.ContactId).Distinct().ToList();

			List<Contact> contacts = contactChanges.Select(contactChange => Contact.Read(sqlConnection, contactChange.ContactId)).ToList();

			return contacts;
		}

		public static List<ExternalContact> GetExternalContacts(SqlConnection sqlConnection, Guid contactId, Guid changeProviderId)
		{
			List<ContactChange> contactChanges = Read(sqlConnection, contactId, IdType.ContactId);

			List<Guid> externalContactIds = contactChanges.Select(contactChange => contactChange.ExternalContactId).Distinct().ToList();

			List<ExternalContact> externalContacts = externalContactIds.Select(externalContactId => ExternalContact.Read(sqlConnection, externalContactId, changeProviderId)).ToList();

			return externalContacts;
		}

		public static List<ExternalContact> GetExternalContacts(SqlConnection sqlConnection, Guid contactId)
		{
			List<ContactChange> contactChanges = Read(sqlConnection, contactId, IdType.ContactId).Distinct().ToList();

			List<ExternalContact> externalContacts = contactChanges.Select(contactChange => ExternalContact.Read(sqlConnection, contactChange.ExternalContactId, contactChange.ChangeProviderId)).ToList();

			return externalContacts;
		}

		public static DateTime GetLatestModifiedOn(SqlConnection sqlConnection, Guid changeProviderId)
		{
			string tableName = typeof(ContactChange).Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	COALESCE(MAX(ModifiedOn),'2000-01-01') ModifiedOn");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + tableName);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ContactChange.ChangeProviderId = @ChangeProviderId");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId));

			DataRow row = dataTable.Rows[0];
			DateTime modifiedOn = (DateTime)row["ModifiedOn"];

			return modifiedOn;
		}
	}
}
