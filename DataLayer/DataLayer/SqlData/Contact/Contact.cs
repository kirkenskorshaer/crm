using DataLayer.MongoData;
using DataLayer.SqlData.Account;
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
	public class Contact : AbstractIdData, IModifiedIdData
	{
		public DateTime createdon;
		public DateTime modifiedon { get; set; }
		public string firstname;
		public string middlename;
		public string lastname;

		public DateTime? birthdate;

		public string address1_line1;
		public string address1_line2;
		public string address1_city;
		public string address1_postalcode;
		public string emailaddress1;
		public string mobilephone;
		public string telephone1;

		public string cprnr;

		public string storkredsnavn;
		public int storkredsnr;
		public string kkadminsoegenavn;
		public DateTime? gavebrevudloebsdato;
		public string titel;
		public bool hargavebrev;
		public bool kkadminstatus;
		public int? new_kkadminmedlemsnr;

		private static readonly List<string> _fields = new List<string>()
		{
			"firstname",
			"middlename",
			"lastname",
			"createdon",
			"modifiedon",

			"birthdate",
			"address1_line1",
			"address1_line2",
			"address1_city",
			"address1_postalcode",
			"emailaddress1",
			"mobilephone",
			"telephone1",
			"cprnr",
			"storkredsnavn",
			"storkredsnr",
			"kkadminsoegenavn",
			"gavebrevudloebsdato",
			"titel",
			"hargavebrev",
			"kkadminstatus",
			"new_kkadminmedlemsnr",
		};

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			string tableName = typeof(Contact).Name;

			List<string> columnsInDatabase = SqlUtilities.GetExistingColumns(sqlConnection, tableName);

			if (columnsInDatabase.Any() == false)
			{
				SqlUtilities.CreateTable(sqlConnection, tableName, "id");
			}

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "firstname", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "middlename", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "lastname", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "createdon", SqlUtilities.DataType.DATETIME, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "modifiedon", SqlUtilities.DataType.DATETIME, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "birthdate", SqlUtilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line1", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line2", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_city", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_postalcode", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "emailaddress1", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "mobilephone", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "telephone1", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "cprnr", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "storkredsnavn", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "storkredsnr", SqlUtilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminsoegenavn", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "gavebrevudloebsdato", SqlUtilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "titel", SqlUtilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "hargavebrev", SqlUtilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminstatus", SqlUtilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_kkadminmedlemsnr", SqlUtilities.DataType.INT, SqlBoolean.True);
		}

		public void Insert(SqlConnection sqlConnection, MongoConnection mongoConnection = null)
		{
			StringBuilder sqlStringBuilderColumns = new StringBuilder();
			StringBuilder sqlStringBuilderParameters = new StringBuilder();
			List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

			AddInsertParameterIfNotNull(firstname, "firstname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(middlename, "middlename", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(lastname, "lastname", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(modifiedon, "modifiedon", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(createdon, "createdon", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

			AddInsertParameterIfNotNull(birthdate, "birthdate", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_line1, "address1_line1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_line2, "address1_line2", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_city, "address1_city", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(address1_postalcode, "address1_postalcode", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(emailaddress1, "emailaddress1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(mobilephone, "mobilephone", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(telephone1, "telephone1", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(cprnr, "cprnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(storkredsnavn, "storkredsnavn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(storkredsnr, "storkredsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminsoegenavn, "kkadminsoegenavn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(gavebrevudloebsdato, "gavebrevudloebsdato", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(titel, "titel", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(hargavebrev, "hargavebrev", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminstatus, "kkadminstatus", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_kkadminmedlemsnr, "new_kkadminmedlemsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, parameters.ToArray());

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

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("lastSearchDate", lastSearchDate));

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

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

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
			AddUpdateParameter(firstname, "firstname", sqlStringBuilderSets, parameters);
			AddUpdateParameter(middlename, "middlename", sqlStringBuilderSets, parameters);
			AddUpdateParameter(lastname, "lastname", sqlStringBuilderSets, parameters);
			AddUpdateParameter(modifiedon, "modifiedon", sqlStringBuilderSets, parameters);
			AddUpdateParameter(createdon, "createdon", sqlStringBuilderSets, parameters);

			AddUpdateParameter(birthdate, "birthdate", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_line1, "address1_line1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_line2, "address1_line2", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_city, "address1_city", sqlStringBuilderSets, parameters);
			AddUpdateParameter(address1_postalcode, "address1_postalcode", sqlStringBuilderSets, parameters);
			AddUpdateParameter(emailaddress1, "emailaddress1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(mobilephone, "mobilephone", sqlStringBuilderSets, parameters);
			AddUpdateParameter(telephone1, "telephone1", sqlStringBuilderSets, parameters);
			AddUpdateParameter(cprnr, "cprnr", sqlStringBuilderSets, parameters);
			AddUpdateParameter(storkredsnavn, "storkredsnavn", sqlStringBuilderSets, parameters);
			AddUpdateParameter(storkredsnr, "storkredsnr", sqlStringBuilderSets, parameters);
			AddUpdateParameter(kkadminsoegenavn, "kkadminsoegenavn", sqlStringBuilderSets, parameters);
			AddUpdateParameter(gavebrevudloebsdato, "gavebrevudloebsdato", sqlStringBuilderSets, parameters);
			AddUpdateParameter(titel, "titel", sqlStringBuilderSets, parameters);
			AddUpdateParameter(hargavebrev, "hargavebrev", sqlStringBuilderSets, parameters);
			AddUpdateParameter(kkadminstatus, "kkadminstatus", sqlStringBuilderSets, parameters);
			AddUpdateParameter(new_kkadminmedlemsnr, "new_kkadminmedlemsnr", sqlStringBuilderSets, parameters);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("Update");
			sqlStringBuilder.AppendLine("	" + TableName);
			sqlStringBuilder.AppendLine("SET");
			sqlStringBuilder.Append(sqlStringBuilderSets);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			parameters.Add(new KeyValuePair<string, object>("id", Id));

			SqlUtilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.Text, parameters.ToArray());
		}

		private static Contact CreateFromRow(DataRow row)
		{
			return new Contact
			{
				firstname = ConvertFromDatabaseValue<string>(row["firstname"]),
				middlename = ConvertFromDatabaseValue<string>(row["middlename"]),
				lastname = ConvertFromDatabaseValue<string>(row["lastname"]),
				modifiedon = ConvertFromDatabaseValue<DateTime>(row["modifiedon"]),
				createdon = ConvertFromDatabaseValue<DateTime>(row["createdon"]),
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
				storkredsnavn = ConvertFromDatabaseValue<string>(row["storkredsnavn"]),
				storkredsnr = ConvertFromDatabaseValue<int>(row["storkredsnr"]),
				kkadminsoegenavn = ConvertFromDatabaseValue<string>(row["kkadminsoegenavn"]),
				gavebrevudloebsdato = ConvertFromDatabaseValue<DateTime?>(row["gavebrevudloebsdato"]),
				titel = ConvertFromDatabaseValue<string>(row["titel"]),
				hargavebrev = ConvertFromDatabaseValue<bool>(row["hargavebrev"]),
				kkadminstatus = ConvertFromDatabaseValue<bool>(row["kkadminstatus"]),
				new_kkadminmedlemsnr = ConvertFromDatabaseValue<int?>(row["new_kkadminmedlemsnr"]),
			};
		}

		public static bool Exists(SqlConnection sqlConnection, Guid id)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT NULL FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	id = @id");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

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

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", contactId));

			DataRow row = dataTable.Rows[0];
			Contact contact = CreateFromRow(row);

			return contact;
		}

		public static Guid? ReadIdFromField(SqlConnection sqlConnection, string fieldName, string fieldValue)
		{
			if (_fields.Contains(fieldName) == false)
			{
				throw new Exception($"Unknown fieldName {fieldName}");
			}

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	id");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(Contact).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{fieldName} = @{fieldName}");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(fieldName, fieldValue));

			if (dataTable.Rows.Count == 0)
			{
				return null;
			}

			DataRow row = dataTable.Rows[0];
			Guid contactId = ConvertFromDatabaseValue<Guid>(row["id"]);

			return contactId;
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

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(foreignKeyName, foreignKeyId));

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
			sqlStringBuilder.AppendLine("	firstname = @firstname");

			DataTable dataTable = SqlUtilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("firstname", firstName));

			List<Contact> contacts = new List<Contact>();

			foreach (DataRow row in dataTable.Rows)
			{
				Contact contact = CreateFromRow(row);

				contacts.Add(contact);
			}

			return contacts;
		}

		public void SynchronizeGroups(SqlConnection sqlConnection, List<Guid> groupIds)
		{
			List<ContactGroup> contactGroups = ContactGroup.ReadFromContactId(sqlConnection, Id);

			foreach (ContactGroup contactGroup in contactGroups)
			{
				if (groupIds.Contains(contactGroup.GroupId) == false)
				{
					contactGroup.Delete(sqlConnection);
				}
			}

			foreach (Guid groupId in groupIds)
			{
				if (contactGroups.Any(contactGroup => contactGroup.GroupId == groupId) == false)
				{
					ContactGroup contactGroup = new ContactGroup(Id, groupId);
					contactGroup.Insert(sqlConnection);
				}
			}
		}
	}
}
