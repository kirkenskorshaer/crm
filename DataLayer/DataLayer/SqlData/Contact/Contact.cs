using DataLayer.MongoData;
using DataLayer.SqlData.Group;
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
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;

		public DateTime? birthdate;

		public string address1_line1;
		public string address1_line2;
		public string address1_city;
		public string address1_postalcode;
		public string emailaddress1;
		public string mobilephone;
		public string telephone1;

		public string cprnr;

		public int kkadminmedlemsnr;
		public string storkredsnavn;
		public int storkredsnr;
		public string kkadminsoegenavn;
		public DateTime? gavebrevudloebsdato;
		public string titel;
		public bool hargavebrev;
		public bool kkadminstatus;

		private static readonly List<string> _fields = new List<string>()
		{
			"FirstName",
			"LastName",
			"CreatedOn",
			"ModifiedOn",

			"birthdate",
			"address1_line1",
			"address1_line2",
			"address1_city",
			"address1_postalcode",
			"emailaddress1",
			"mobilephone",
			"telephone1",
			"cprnr",
			"kkadminmedlemsnr",
			"storkredsnavn",
			"storkredsnr",
			"kkadminsoegenavn",
			"gavebrevudloebsdato",
			"titel",
			"hargavebrev",
			"kkadminstatus",
		};

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

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "birthdate", Utilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line2", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_city", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_postalcode", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "emailaddress1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "mobilephone", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "telephone1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "cprnr", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminmedlemsnr", Utilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "storkredsnavn", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "storkredsnr", Utilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminsoegenavn", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "gavebrevudloebsdato", Utilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "titel", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "hargavebrev", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminstatus", Utilities.DataType.BIT, SqlBoolean.True);
		}

		public void Insert(SqlConnection sqlConnection, MongoConnection mongoConnection = null)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			AddInsertParameterIfNotNull(Firstname, "Firstname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(Lastname, "Lastname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(ModifiedOn, "ModifiedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(CreatedOn, "CreatedOn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(birthdate, "birthdate", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_line1, "address1_line1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_line2, "address1_line2", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_city, "address1_city", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_postalcode, "address1_postalcode", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(emailaddress1, "emailaddress1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(mobilephone, "mobilephone", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(telephone1, "telephone1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(cprnr, "cprnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminmedlemsnr, "kkadminmedlemsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(storkredsnavn, "storkredsnavn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(storkredsnr, "storkredsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminsoegenavn, "kkadminsoegenavn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(gavebrevudloebsdato, "gavebrevudloebsdato", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(titel, "titel", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(hargavebrev, "hargavebrev", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminstatus, "kkadminstatus", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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

			CreateProgressForContact(mongoConnection);
		}

		private void CreateProgressForContact(MongoConnection mongoConnection)
		{
			if (mongoConnection == null)
			{
				return;
			}

			string progressName = "Contact";

			if (Progress.Exists(mongoConnection, progressName, Id))
			{
				return;
			}

			Progress newContactProgress = new Progress()
			{
				LastProgressDate = DateTime.Now,
				TargetId = Id,
				TargetName = progressName,
			};

			newContactProgress.Insert(mongoConnection);
		}

		public static List<Contact> ReadLatest(SqlConnection sqlConnection, DateTime lastSearchDate)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

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

		private static void AddFieldsToStringBuilder(StringBuilder sqlStringBuilder)
		{
			_fields.ForEach(field => sqlStringBuilder.AppendLine($"	,[Contact].{field}"));
		}

		public static Contact ReadNextById(SqlConnection sqlConnection, Guid id)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT TOP 1");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	contact.id > @id");
			sqlStringBuilder.AppendLine("ORDER BY");
			sqlStringBuilder.AppendLine("	contact.id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			if (dataTable.Rows.Count == 1)
			{
				DataRow row = dataTable.Rows[0];

				Contact contact = CreateFromRow(row);

				return contact;
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
			AddUpdateParameter(Firstname, "Firstname", sqlStringBuilderSets, parameters);
			AddUpdateParameter(Lastname, "Lastname", sqlStringBuilderSets, parameters);
			AddUpdateParameter(ModifiedOn, "ModifiedOn", sqlStringBuilderSets, parameters);
			AddUpdateParameter(CreatedOn, "CreatedOn", sqlStringBuilderSets, parameters);

			AddUpdateParameter(birthdate, "birthdate", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_line1, "address1_line1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_line2, "address1_line2", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_city, "address1_city", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_postalcode, "address1_postalcode", sqlStringBuilderSets, parameters);
			AddUpdateParameter(emailaddress1, "emailaddress1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(mobilephone, "mobilephone", sqlStringBuilderSets, parameters);
			AddUpdateParameter(telephone1, "telephone1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(cprnr, "cprnr", sqlStringBuilderSets, parameters);
			AddUpdateParameter(kkadminmedlemsnr, "kkadminmedlemsnr", sqlStringBuilderSets, parameters);
			AddUpdateParameter(storkredsnavn, "storkredsnavn", sqlStringBuilderSets, parameters);
			AddUpdateParameter(storkredsnr, "storkredsnr", sqlStringBuilderSets, parameters);
			AddUpdateParameter(kkadminsoegenavn, "kkadminsoegenavn", sqlStringBuilderSets, parameters);
			AddUpdateParameter(gavebrevudloebsdato, "gavebrevudloebsdato", sqlStringBuilderSets, parameters);
			AddUpdateParameter(titel, "titel", sqlStringBuilderSets, parameters);
			AddUpdateParameter(hargavebrev, "hargavebrev", sqlStringBuilderSets, parameters);
			AddUpdateParameter(kkadminstatus, "kkadminstatus", sqlStringBuilderSets, parameters);

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

		private static Contact CreateFromRow(DataRow row)
		{
			return new Contact
			{
				Firstname = ConvertFromDatabaseValue<string>(row["Firstname"]),
				Lastname = ConvertFromDatabaseValue<string>(row["LastName"]),
				ModifiedOn = ConvertFromDatabaseValue<DateTime>(row["ModifiedOn"]),
				CreatedOn = ConvertFromDatabaseValue<DateTime>(row["CreatedOn"]),
				Id = ConvertFromDatabaseValue<Guid>(row["id"]),

				birthdate = ConvertFromDatabaseValue<DateTime?>(row["birthdate"]),
				address1_line1 = ConvertFromDatabaseValue<string>(row["address1_line1"]),
				address1_line2 = ConvertFromDatabaseValue<string>(row["address1_line2"]),
				address1_city = ConvertFromDatabaseValue<string>(row["address1_city"]),
				address1_postalcode = ConvertFromDatabaseValue<string>(row["address1_postalcode"]),
				emailaddress1 = ConvertFromDatabaseValue<string>(row["emailaddress1"]),
				mobilephone = ConvertFromDatabaseValue<string>(row["mobilephone"]),
				telephone1 = ConvertFromDatabaseValue<string>(row["telephone1"]),
				cprnr = ConvertFromDatabaseValue<string>(row["cprnr"]),
				kkadminmedlemsnr = ConvertFromDatabaseValue<int>(row["kkadminmedlemsnr"]),
				storkredsnavn = ConvertFromDatabaseValue<string>(row["storkredsnavn"]),
				storkredsnr = ConvertFromDatabaseValue<int>(row["storkredsnr"]),
				kkadminsoegenavn = ConvertFromDatabaseValue<string>(row["kkadminsoegenavn"]),
				gavebrevudloebsdato = ConvertFromDatabaseValue<DateTime?>(row["gavebrevudloebsdato"]),
				titel = ConvertFromDatabaseValue<string>(row["titel"]),
				hargavebrev = ConvertFromDatabaseValue<bool>(row["hargavebrev"]),
				kkadminstatus = ConvertFromDatabaseValue<bool>(row["kkadminstatus"]),
			};
		}

		public static bool Exists(SqlConnection sqlConnection, Guid id)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT NULL FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			return dataTable.Rows.Count > 0;
		}

		public static Contact Read(SqlConnection sqlConnection, Guid contactId)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", contactId));

			DataRow row = dataTable.Rows[0];
			Contact contact = CreateFromRow(row);

			return contact;
		}

		public static List<Contact> ReadContactsFromAccountContact(SqlConnection sqlConnection, Guid accountId)
		{
			return ReadContacts(sqlConnection, accountId, "AccountId", typeof(AccountContact));
		}

		public static List<Contact> ReadContactsFromAccountChangeContact(SqlConnection sqlConnection, Guid accountChangeId)
		{
			return ReadContacts(sqlConnection, accountChangeId, "AccountChangeId", typeof(AccountChangeContact));
		}

		public static List<Contact> ReadContactsFromContactGroup(SqlConnection sqlConnection, Guid groupId)
		{
			return ReadContacts(sqlConnection, groupId, "GroupId", typeof(ContactGroup));
		}

		public static List<Contact> ReadContactsFromAccountIndsamler(SqlConnection sqlConnection, Guid accountId)
		{
			return ReadContacts(sqlConnection, accountId, "AccountId", typeof(AccountIndsamler));
		}

		public static List<Contact> ReadContactsFromAccountChangeIndsamler(SqlConnection sqlConnection, Guid accountChangeId)
		{
			return ReadContacts(sqlConnection, accountChangeId, "AccountChangeId", typeof(AccountChangeIndsamler));
		}

		private static List<Contact> ReadContacts(SqlConnection sqlConnection, Guid foreignKeyId, string foreignKeyName, Type NNTable)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			AddFieldsToStringBuilder(sqlStringBuilder);
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{typeof(Contact).Name}]");
			sqlStringBuilder.AppendLine("JOIN");
			sqlStringBuilder.AppendLine($"	{NNTable.Name}");
			sqlStringBuilder.AppendLine("ON");
			sqlStringBuilder.AppendLine($"	{NNTable.Name}.ContactId = [{typeof(Contact).Name}].id");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{NNTable.Name}.{foreignKeyName} = @{foreignKeyName}");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(foreignKeyName, foreignKeyId));

			List<Contact> contacts = ReadContactsFromDataTable(dataTable);

			return contacts;
		}

		private static List<Contact> ReadContactsFromDataTable(DataTable dataTable)
		{
			List<Contact> contacts = new List<Contact>();

			foreach (DataRow row in dataTable.Rows)
			{
				Contact contact = CreateFromRow(row);

				contacts.Add(contact);
			}

			return contacts;
		}

		public static List<Contact> Read(SqlConnection sqlConnection, string firstName)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");

			AddFieldsToStringBuilder(sqlStringBuilder);

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
