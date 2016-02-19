using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData
{
	public class ChangeProvider : AbstractIdData
	{
		public string Name;

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(ChangeProvider).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "Name", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
		}

		public void Insert(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("INSERT INTO");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	Name");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("OUTPUT");
			sqlStringBuilder.AppendLine("	Inserted.id");
			sqlStringBuilder.AppendLine("VALUES");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	@name");
			sqlStringBuilder.AppendLine(")");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("name", Name));

			DataRow row = dataTable.Rows[0];
			Id = (Guid)row["id"];
		}

		public static List<ChangeProvider> ReadAll(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Name");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ChangeProvider).Name);

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder);

			List<ChangeProvider> contactChangeProviders = new List<ChangeProvider>();

			foreach (DataRow row in dataTable.Rows)
			{
				ChangeProvider changeProvider = CreateFromDataRow(row);
				contactChangeProviders.Add(changeProvider);
			}

			return contactChangeProviders;
		}

		private static ChangeProvider CreateFromDataRow(DataRow row)
		{
			return new ChangeProvider
			{
				Name = (string)row["Name"],
				Id = (Guid)row["id"],
			};
		}

		public static ChangeProvider Read(SqlConnection sqlConnection, Guid changeProviderId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Name");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ChangeProvider).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", changeProviderId));

			DataRow row = dataTable.Rows[0];
			ChangeProvider changeProvider = CreateFromDataRow(row);

			return changeProvider;
		}

		public static ChangeProvider ReadByName(SqlConnection sqlConnection, string name)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("	,Name");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ChangeProvider).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	Name = @name");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("name", name));

			DataRow row = dataTable.Rows[0];
			ChangeProvider changeProvider = CreateFromDataRow(row);

			return changeProvider;
		}

		public static bool ExistsByName(SqlConnection sqlConnection, string name)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	NULL");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(ChangeProvider).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	Name = @name");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("name", name));

			bool exists = dataTable.Rows.Count >= 1;

			return exists;
		}

		public static ChangeProvider ReadByNameOrCreate(SqlConnection sqlConnection, string name)
		{
			bool exists = ExistsByName(sqlConnection, name);

			if (exists)
			{
				return ReadByName(sqlConnection, name);
			}

			ChangeProvider changeProvider = new ChangeProvider()
			{
				Name = name,
			};

			changeProvider.Insert(sqlConnection);

			return changeProvider;
		}
	}
}
