﻿using DataLayer.MongoData;
using DataLayer.SqlData.Group;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Account
{
	public class Account : AbstractIdData, IModifiedIdData
	{
		public DateTime CreatedOn;
		public DateTime ModifiedOn { get; set; }

		public string name;

		public string address1_line1;
		public string address1_line2;
		public string address1_city;
		public string address1_postalcode;
		public string emailaddress1;
		public string telephone1;

		public bool new_erindsamlingssted;
		public int new_kkadminmedlemsnr;
		public string new_region;

		private static readonly List<string> _fields = new List<string>()
		{
			"createdon",
			"modifiedon",

			"name",

			"address1_line1",
			"address1_line2",
			"address1_city",
			"address1_postalcode",
			"emailaddress1",
			"telephone1",

			"new_erindsamlingssted",
			"new_kkadminmedlemsnr",
			"new_region",
		};

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(Account).Name;

			List<string> columnsInDatabase = Utilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				Utilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "name", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "CreatedOn", Utilities.DataType.DATETIME, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ModifiedOn", Utilities.DataType.DATETIME, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line2", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_city", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_postalcode", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "emailaddress1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "mobilephone", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "telephone1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_erindsamlingssted", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_kkadminmedlemsnr", Utilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_region", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
		}

		public void Insert(SqlConnection sqlConnection, MongoConnection mongoConnection = null)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			AddInsertParameterIfNotNull(name, "name", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ModifiedOn, "ModifiedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(CreatedOn, "CreatedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(address1_line1, "address1_line1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_line2, "address1_line2", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_city, "address1_city", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_postalcode, "address1_postalcode", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(emailaddress1, "emailaddress1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(telephone1, "telephone1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(new_erindsamlingssted, "new_erindsamlingssted", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_erindsamlingssted, "new_kkadminmedlemsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_erindsamlingssted, "new_region", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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

			CreateProgressForAccount(mongoConnection);
		}

		private void CreateProgressForAccount(MongoConnection mongoConnection)
		{
			if (mongoConnection == null)
			{
				return;
			}

			string progressName = "Account";

			if (Progress.Exists(mongoConnection, progressName, Id))
			{
				return;
			}

			Progress newAccountProgress = new Progress()
			{
				LastProgressDate = DateTime.Now,
				TargetId = Id,
				TargetName = progressName,
			};

			newAccountProgress.Insert(mongoConnection);
		}

		private static void AddFieldsToStringBuilder(StringBuilder sqlStringBuilder)
		{
			_fields.ForEach(field => sqlStringBuilder.AppendLine($"	,{field}"));
		}

		public static Account ReadNextById(SqlConnection sqlConnection, Guid id)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT TOP 1");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Account).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	account.id > @id");
			sqlStringBuilder.AppendLine("ORDER BY");
			sqlStringBuilder.AppendLine("	account.id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			if (dataTable.Rows.Count == 1)
			{
				DataRow row = dataTable.Rows[0];

				Account account = CreateFromRow(row);

				return account;
			}

			if (id == Guid.Empty)
			{
				return null;
			}

			return ReadNextById(sqlConnection, Guid.Empty);
		}

		public void Update(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilderSets = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
			AddUpdateParameter(name, "name", sqlStringBuilderSets, parameters);
			AddUpdateParameter(ModifiedOn, "ModifiedOn", sqlStringBuilderSets, parameters);
			AddUpdateParameter(CreatedOn, "CreatedOn", sqlStringBuilderSets, parameters);

			AddUpdateParameter(address1_line1, "address1_line1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_line2, "address1_line2", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_city, "address1_city", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_postalcode, "address1_postalcode", sqlStringBuilderSets, parameters);
			AddUpdateParameter(emailaddress1, "emailaddress1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(telephone1, "telephone1", sqlStringBuilderSets, parameters);

			AddUpdateParameter(new_erindsamlingssted, "new_erindsamlingssted", sqlStringBuilderSets, parameters);
			AddUpdateParameter(new_erindsamlingssted, "new_kkadminmedlemsnr", sqlStringBuilderSets, parameters);
			AddUpdateParameter(new_erindsamlingssted, "new_region", sqlStringBuilderSets, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("Update");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("SET");
			sqlStringBuilder.Append(sqlStringBuilderSets);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			parameters.Add(new KeyValuePair<string, object>("id", Id));

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
		}

		private static Account CreateFromRow(DataRow row)
		{
			return new Account
			{
				name = ConvertFromDatabaseValue<string>(row["name"]),
				ModifiedOn = ConvertFromDatabaseValue<DateTime>(row["ModifiedOn"]),
				CreatedOn = ConvertFromDatabaseValue<DateTime>(row["CreatedOn"]),
				Id = ConvertFromDatabaseValue<Guid>(row["id"]),

				address1_line1 = ConvertFromDatabaseValue<string>(row["address1_line1"]),
				address1_line2 = ConvertFromDatabaseValue<string>(row["address1_line2"]),
				address1_city = ConvertFromDatabaseValue<string>(row["address1_city"]),
				address1_postalcode = ConvertFromDatabaseValue<string>(row["address1_postalcode"]),
				emailaddress1 = ConvertFromDatabaseValue<string>(row["emailaddress1"]),
				telephone1 = ConvertFromDatabaseValue<string>(row["telephone1"]),

				new_erindsamlingssted = ConvertFromDatabaseValue<bool>(row["new_erindsamlingssted"]),
				new_kkadminmedlemsnr = ConvertFromDatabaseValue<int>(row["new_kkadminmedlemsnr"]),
				new_region = ConvertFromDatabaseValue<string>(row["new_region"]),
			};
		}

		public static bool Exists(SqlConnection sqlConnection, Guid id)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT NULL FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Account).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			return dataTable.Rows.Count > 0;
		}

		public static Account Read(SqlConnection sqlConnection, Guid accountId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Account).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", accountId));

			DataRow row = dataTable.Rows[0];
			Account account = CreateFromRow(row);

			return account;
		}

		public static List<Account> ReadLatest(SqlConnection sqlConnection, DateTime lastSearchDate)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Account).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	ModifiedOn >= @lastSearchDate");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("lastSearchDate", lastSearchDate));

			List<Account> Accounts = new List<Account>();

			foreach (DataRow row in dataTable.Rows)
			{
				Account Account = CreateFromRow(row);

				Accounts.Add(Account);
			}

			return Accounts;
		}

		public void SynchronizeGroups(SqlConnection sqlConnection, List<Guid> groupIds)
		{
			List<AccountGroup> accountGroups = AccountGroup.ReadFromAccountId(sqlConnection, Id);

			foreach (AccountGroup accountGroup in accountGroups)
			{
				if (groupIds.Contains(accountGroup.GroupId) == false)
				{
					accountGroup.Delete(sqlConnection);
				}
			}

			foreach (Guid groupId in groupIds)
			{
				if (accountGroups.Any(accountGroup => accountGroup.GroupId == groupId) == false)
				{
					AccountGroup accountGroup = new AccountGroup(Id, groupId);
					accountGroup.Insert(sqlConnection);
				}
			}
		}

		public void SynchronizeContacts(SqlConnection sqlConnection, List<Guid> contactIds)
		{
			List<AccountContact> accountContacts = AccountContact.ReadFromAccountId(sqlConnection, Id);

			foreach (AccountContact accountContact in accountContacts)
			{
				if (contactIds.Contains(accountContact.ContactId) == false)
				{
					accountContact.Delete(sqlConnection);
				}
			}

			foreach (Guid contactId in contactIds)
			{
				if (accountContacts.Any(accountContact => accountContact.ContactId == contactId) == false)
				{
					AccountContact accountContact = new AccountContact(Id, contactId);
					accountContact.Insert(sqlConnection);
				}
			}
		}

		public void SynchronizeIndsamlere(SqlConnection sqlConnection, List<Guid> contactIds)
		{
			List<AccountIndsamler> accountIndsamlere = AccountIndsamler.ReadFromAccountId(sqlConnection, Id);

			foreach (AccountIndsamler accountIndsamler in accountIndsamlere)
			{
				if (contactIds.Contains(accountIndsamler.ContactId) == false)
				{
					accountIndsamler.Delete(sqlConnection);
				}
			}

			foreach (Guid contactId in contactIds)
			{
				if (accountIndsamlere.Any(accountContact => accountContact.ContactId == contactId) == false)
				{
					AccountIndsamler accountGroup = new AccountIndsamler(Id, contactId);
					accountGroup.Insert(sqlConnection);
				}
			}
		}
	}
}
