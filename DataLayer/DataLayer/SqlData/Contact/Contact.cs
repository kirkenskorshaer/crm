using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Contact
{
	public class Contact : AbstractIdData
	{
		public Guid ContactId;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(Contact).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "FirstName", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "LastName", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "CreatedOn", Utilities.DataType.DATETIME, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ModifiedOn", Utilities.DataType.DATETIME, SqlBoolean.False);
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddInsertParameterIfNotNull(Firstname, "Firstname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(Lastname, "Lastname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ModifiedOn, "ModifiedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(CreatedOn, "CreatedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());

			DataRow row = dataTable.Rows[0];
			Id = (Guid)row["id"];
		}

		public static List<Contact> ReadLatest(SqlConnection sqlConnection, DateTime lastSearchDate)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Firstname");
			sqlStringBuilder.AppendLine("	,Lastname");
			sqlStringBuilder.AppendLine("	,ModifiedOn");
			sqlStringBuilder.AppendLine("	,CreatedOn");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ModifiedOn >= @lastSearchDate");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("lastSearchDate", lastSearchDate));

			List<Contact> contacts = new List<Contact>();

			foreach (DataRow row in dataTable.Rows)
			{
				Contact contact = CreateFromRow(row);

				contacts.Add(contact);
			}

			return contacts;
		}

		private static Contact CreateFromRow(DataRow row)
		{
			return new Contact
			{
				Firstname = ConvertFromDatabaseValue<string>(row["Firstname"]),
				Lastname = ConvertFromDatabaseValue<string>(row["LastName"]),
				ModifiedOn = ConvertFromDatabaseValue<DateTime>(row["ModifiedOn"]),
				CreatedOn = ConvertFromDatabaseValue<DateTime>(row["CreatedOn"]),
				Id = ConvertFromDatabaseValue<Guid>(row["id"]),
			};
		}

		public static Contact Read(SqlConnection sqlConnection, Guid contactId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Firstname");
			sqlStringBuilder.AppendLine("	,Lastname");
			sqlStringBuilder.AppendLine("	,ModifiedOn");
			sqlStringBuilder.AppendLine("	,CreatedOn");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", contactId));

			DataRow row = dataTable.Rows[0];
			Contact contact = CreateFromRow(row);

			return contact;
		}

		public static List<Contact> Read(SqlConnection sqlConnection, string firstName)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Firstname");
			sqlStringBuilder.AppendLine("	,Lastname");
			sqlStringBuilder.AppendLine("	,ModifiedOn");
			sqlStringBuilder.AppendLine("	,CreatedOn");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	Firstname = @Firstname");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("Firstname", firstName));

			List<Contact> contacts = new List<Contact>();

			foreach (DataRow row in dataTable.Rows)
			{
				Contact contact = CreateFromRow(row);

				contacts.Add(contact);
			}

			return contacts;
		}
	}
}
