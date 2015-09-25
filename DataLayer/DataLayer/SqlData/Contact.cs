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
	}
}
