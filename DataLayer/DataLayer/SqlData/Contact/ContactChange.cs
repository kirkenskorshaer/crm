using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace DataLayer.SqlData.Contact
{
	public class ContactChange : AbstractIdData, IModifiedIdData
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

		public int kkadminmedlemsnr;
		public string storkredsnavn;
		public int storkredsnr;
		public string kkadminsoegenavn;
		public DateTime? gavebrevudloebsdato;
		public string titel;
		public bool hargavebrev;
		public bool kkadminstatus;
		public bool new_bykoordinator;
		public bool new_omraadekoordinator;
		public bool new_korshaersleder;
		public int new_kkadminmedlemsnr;

		public Guid ExternalContactId { get; private set; }
		public Guid ChangeProviderId { get; private set; }
		public Guid ContactId { get; private set; }

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
			"kkadminmedlemsnr",
			"storkredsnavn",
			"storkredsnr",
			"kkadminsoegenavn",
			"gavebrevudloebsdato",
			"titel",
			"hargavebrev",
			"kkadminstatus",
			"new_bykoordinator",
			"new_omraadekoordinator",
			"new_korshaersleder",
			"new_kkadminmedlemsnr",
		};

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

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ExternalContactId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ChangeProviderId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "ContactId", Utilities.DataType.UNIQUEIDENTIFIER, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "firstname", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "middlename", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "lastname", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "createdon", Utilities.DataType.DATETIME, SqlBoolean.False);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "modifiedon", Utilities.DataType.DATETIME, SqlBoolean.False);

			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "birthdate", Utilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_line2", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_city", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "address1_postalcode", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "emailaddress1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "mobilephone", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "telephone1", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "cprnr", Utilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminmedlemsnr", Utilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "storkredsnavn", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "storkredsnr", Utilities.DataType.INT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminsoegenavn", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "gavebrevudloebsdato", Utilities.DataType.DATETIME, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "titel", Utilities.DataType.NVARCHAR_MAX, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "hargavebrev", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "kkadminstatus", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_bykoordinator", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_omraadekoordinator", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_korshaersleder", Utilities.DataType.BIT, SqlBoolean.True);
			CreateIfMissing(sqlConnection, tableName, columnsInDatabase, "new_kkadminmedlemsnr", Utilities.DataType.INT, SqlBoolean.True);

			CreateKeyIfMissing(sqlConnection, _tableName, "ContactId", typeof(Contact).Name, "id");

			Utilities.MaintainCompositeForeignKey3Keys(sqlConnection, tableName, "ChangeProviderId", "ExternalContactId", "ContactId", typeof(ExternalContact).Name, "ChangeProviderId", "ExternalContactId", "ContactId");
		}

		public void Insert()
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
			AddInsertParameterIfNotNull(kkadminmedlemsnr, "kkadminmedlemsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(storkredsnavn, "storkredsnavn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(storkredsnr, "storkredsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminsoegenavn, "kkadminsoegenavn", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(gavebrevudloebsdato, "gavebrevudloebsdato", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(titel, "titel", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(hargavebrev, "hargavebrev", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(kkadminstatus, "kkadminstatus", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_bykoordinator, "new_bykoordinator", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_omraadekoordinator, "new_omraadekoordinator", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_korshaersleder, "new_korshaersleder", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);
			AddInsertParameterIfNotNull(new_kkadminmedlemsnr, "new_kkadminmedlemsnr", sqlStringBuilderColumns, sqlStringBuilderParameters, parameters);

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

		public static bool ContactChangeExists(SqlConnection sqlConnection, Guid contactId, Guid externalContactId, Guid changeProviderId, DateTime modifiedOn)
		{
			string tableName = typeof(ContactChange).Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT TOP 1");
			sqlStringBuilder.AppendLine("	NULL");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	EXISTS");
			sqlStringBuilder.AppendLine("	(");
			sqlStringBuilder.AppendLine("		SELECT");
			sqlStringBuilder.AppendLine("			*");
			sqlStringBuilder.AppendLine("		FROM");
			sqlStringBuilder.AppendLine("			" + tableName);
			sqlStringBuilder.AppendLine("		WHERE");
			sqlStringBuilder.AppendLine("			ContactChange.ContactId = @ContactId");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			ContactChange.ExternalContactId = @ExternalContactId");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			ContactChange.ChangeProviderId = @ChangeProviderId");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			CASE");
			sqlStringBuilder.AppendLine("				WHEN");
			sqlStringBuilder.AppendLine("					DATEDIFF(DAY, ModifiedOn, @ModifiedOn) = 0");
			sqlStringBuilder.AppendLine("				THEN");
			sqlStringBuilder.AppendLine("					CASE");
			sqlStringBuilder.AppendLine("						WHEN");
			sqlStringBuilder.AppendLine("							DATEDIFF(MILLISECOND, ModifiedOn, @ModifiedOn) = 0");
			sqlStringBuilder.AppendLine("						THEN");
			sqlStringBuilder.AppendLine("							0");
			sqlStringBuilder.AppendLine("						ELSE");
			sqlStringBuilder.AppendLine("							1");
			sqlStringBuilder.AppendLine("					END");
			sqlStringBuilder.AppendLine("			END = 0");
			sqlStringBuilder.AppendLine("	)");

			DataTable dataTable = Utilities.ExecuteAdapterSelect(sqlConnection, sqlStringBuilder
				, new KeyValuePair<string, object>("ContactId", contactId)
				, new KeyValuePair<string, object>("ExternalContactId", externalContactId)
				, new KeyValuePair<string, object>("ChangeProviderId", changeProviderId)
				, new KeyValuePair<string, object>("ModifiedOn", modifiedOn));

			bool exists = dataTable.Rows.Count == 1;

			return exists;
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

			AddFieldsToStringBuilder(sqlStringBuilder);

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

		private static void AddFieldsToStringBuilder(StringBuilder sqlStringBuilder)
		{
			_fields.ForEach(field => sqlStringBuilder.AppendLine($"	,{field}"));
		}

		private static ContactChange CreateFromRow(SqlConnection sqlConnection, DataRow row)
		{
			Guid externalContactId = (Guid)row["ExternalContactId"];
			Guid changeProviderId = (Guid)row["ChangeProviderId"];
			Guid contactId = (Guid)row["ContactId"];

			ContactChange contactChange = new ContactChange(sqlConnection, contactId, externalContactId, changeProviderId)
			{
				firstname = ConvertFromDatabaseValue<string>(row["firstname"]),
				middlename = ConvertFromDatabaseValue<string>(row["middlename"]),
				lastname = ConvertFromDatabaseValue<string>(row["lastName"]),
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
				kkadminmedlemsnr = ConvertFromDatabaseValue<int>(row["kkadminmedlemsnr"]),
				storkredsnavn = ConvertFromDatabaseValue<string>(row["storkredsnavn"]),
				storkredsnr = ConvertFromDatabaseValue<int>(row["storkredsnr"]),
				kkadminsoegenavn = ConvertFromDatabaseValue<string>(row["kkadminsoegenavn"]),
				gavebrevudloebsdato = ConvertFromDatabaseValue<DateTime?>(row["gavebrevudloebsdato"]),
				titel = ConvertFromDatabaseValue<string>(row["titel"]),
				hargavebrev = ConvertFromDatabaseValue<bool>(row["hargavebrev"]),
				kkadminstatus = ConvertFromDatabaseValue<bool>(row["kkadminstatus"]),
				new_bykoordinator = ConvertFromDatabaseValue<bool>(row["new_bykoordinator"]),
				new_omraadekoordinator = ConvertFromDatabaseValue<bool>(row["new_omraadekoordinator"]),
				new_korshaersleder = ConvertFromDatabaseValue<bool>(row["new_korshaersleder"]),
				new_kkadminmedlemsnr = ConvertFromDatabaseValue<int>(row["new_kkadminmedlemsnr"]),
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
