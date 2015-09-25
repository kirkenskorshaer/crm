using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData
{
	public class Contact
	{
		public Guid ContactId;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;

		private static string _databaseName = "contact";

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, _databaseName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, _databaseName, "id");
			}

			CreateIfMissing(sqlConnection, columnsInDatabase, "FirstName", "NVARCHAR(MAX)", SqlBoolean.True);
			CreateIfMissing(sqlConnection, columnsInDatabase, "LastName", "NVARCHAR(MAX)", SqlBoolean.True);
			CreateIfMissing(sqlConnection, columnsInDatabase, "CreatedOn", "DATETIME", SqlBoolean.False);
			CreateIfMissing(sqlConnection, columnsInDatabase, "ModifiedOn", "DATETIME", SqlBoolean.False);
		}

		private static void CreateIfMissing(SqlConnection sqlConnection, List<string> columnsInDatabase, string name, string type, SqlBoolean allowNull)
		{
			if (columnsInDatabase.Contains(name) == false)
			{
				Utilities.AddColumn(sqlConnection, "contact", name, type, allowNull);
			}
		}

		public static List<Contact> ReadLatest(SqlConnection sqlConnection, DateTime lastSearchDate)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	Firstname");
			sqlStringBuilder.AppendLine("	,Lastname");
			sqlStringBuilder.AppendLine("	,ModifiedOn");
			sqlStringBuilder.AppendLine("	,CreatedOn");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + _databaseName);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ModifiedOn >= @lastSearchDate");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("lastSearchDate", lastSearchDate));

			List<Contact> contacts = new List<Contact>();

			foreach (DataRow row in dataTable.Rows)
			{
				Contact contact = new Contact
				{
					Firstname = (string)row["Firstname"],
					Lastname = (string)row["LastName"],
					ModifiedOn = (DateTime)row["ModifiedOn"],
					CreatedOn = (DateTime)row["CreatedOn"],
				};

				contacts.Add(contact);
			}

			return contacts;
		}
	}
}
