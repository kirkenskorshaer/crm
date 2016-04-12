﻿using DataLayer.SqlData.Account;
using DataLayer.SqlData.Contact;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataLayer.SqlData
{
	public static class SqlUtilities
	{
		public static DataTable ExecuteAdapterSelect(SqlConnection sqlConnection, StringBuilder sqlStringBuilder, params KeyValuePair<string, object>[] parameters)
		{
			SqlDataAdapter adapter = new SqlDataAdapter(sqlStringBuilder.ToString(), sqlConnection);

			for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
			{
				SqlParameter sqlParameter = new SqlParameter(parameters[parameterIndex].Key, parameters[parameterIndex].Value);
				adapter.SelectCommand.Parameters.Add(sqlParameter);
			}

			DataTable columnsTable = new DataTable();
			adapter.Fill(columnsTable);
			return columnsTable;
		}

		public static void ExecuteNonQuery(SqlConnection sqlConnection, StringBuilder sqlStringBuilder, CommandType commandType, params KeyValuePair<string, object>[] parameters)
		{
			SqlCommand sqlCommand = new SqlCommand(sqlStringBuilder.ToString(), sqlConnection);
			sqlCommand.CommandType = commandType;

			for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
			{
				object value = parameters[parameterIndex].Value;

				if (value == null)
				{
					value = DBNull.Value;
				}

				SqlParameter sqlParameter = new SqlParameter(parameters[parameterIndex].Key, value);

				sqlCommand.Parameters.Add(sqlParameter);
			}

			sqlConnection.Open();

			try
			{
				sqlCommand.ExecuteNonQuery();
			}
			finally
			{
				sqlConnection.Close();
			}
		}

		public static List<string> GetExistingColumns(SqlConnection sqlConnection, string tableName)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine("	COLUMNS.TABLE_NAME");
			sqlStringBuilder.AppendLine("	,COLUMNS.COLUMN_NAME");
			sqlStringBuilder.AppendLine("	,COLUMNS.DATA_TYPE");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	INFORMATION_SCHEMA.COLUMNS");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine("	COLUMNS.TABLE_NAME = @tableName");

			DataTable columnsTable = ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("tableName", tableName));

			List<string> columns = columnsTable.Select().Select(row => (string)row["COLUMN_NAME"]).ToList();

			return columns;
		}

		public static void CreateTable(SqlConnection sqlConnection, string tableName, string primaryKeyName)
		{
			Procedures.CreateTable.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateTable");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName", primaryKeyName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void CreateCompositeTable2Tables(SqlConnection sqlConnection, string tableName, string primaryKeyName1, string primaryKeyName2)
		{
			Procedures.CreateCompositeTable2Tables.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateCompositeTable2Tables");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName1", primaryKeyName1),
				new KeyValuePair<string, object>("primaryKeyName2", primaryKeyName2),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void CreateCompositeTable3Tables(SqlConnection sqlConnection, string tableName, string primaryKeyName1, string primaryKeyName2, string primaryKeyName3)
		{
			Procedures.CreateCompositeTable3Tables.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("CreateCompositeTable3Tables");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("primaryKeyName1", primaryKeyName1),
				new KeyValuePair<string, object>("primaryKeyName2", primaryKeyName2),
				new KeyValuePair<string, object>("primaryKeyName3", primaryKeyName3),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainAllTables(SqlConnection sqlConnection)
		{
			ChangeProvider.MaintainTable(sqlConnection);
			Contact.Contact.MaintainTable(sqlConnection);
			ExternalContact.MaintainTable(sqlConnection);
			ContactChange.MaintainTable(sqlConnection);
			Byarbejde.Byarbejde.MaintainTable(sqlConnection);
			Byarbejde.ExternalByarbejde.MaintainTable(sqlConnection);
			Account.Account.MaintainTable(sqlConnection);
			ExternalAccount.MaintainTable(sqlConnection);
			AccountChange.MaintainTable(sqlConnection);
			AccountContact.MaintainTable(sqlConnection);
			AccountChangeContact.MaintainTable(sqlConnection);
			AccountIndsamler.MaintainTable(sqlConnection);
			AccountChangeIndsamler.MaintainTable(sqlConnection);
			Group.Group.MaintainTable(sqlConnection);
			Group.ContactGroup.MaintainTable(sqlConnection);
			Group.ContactChangeGroup.MaintainTable(sqlConnection);
			Group.AccountGroup.MaintainTable(sqlConnection);
			Group.AccountChangeGroup.MaintainTable(sqlConnection);
		}

		public static void DeleteAllTables(SqlConnection sqlConnection)
		{
			DropTable(sqlConnection, typeof(Group.ContactChangeGroup).Name);
			DropTable(sqlConnection, typeof(ContactChange).Name);
			DropTable(sqlConnection, typeof(Group.ContactGroup).Name);
			DropTable(sqlConnection, typeof(AccountContact).Name);
			DropTable(sqlConnection, typeof(AccountIndsamler).Name);
			DropTable(sqlConnection, typeof(AccountChangeIndsamler).Name);
			DropTable(sqlConnection, typeof(AccountChangeContact).Name);
			DropTable(sqlConnection, typeof(ExternalContact).Name);

			DropTable(sqlConnection, typeof(Group.AccountChangeGroup).Name);
			DropTable(sqlConnection, typeof(AccountChange).Name);
			DropTable(sqlConnection, typeof(Group.AccountGroup).Name);
			DropTable(sqlConnection, typeof(ExternalAccount).Name);
			DropTable(sqlConnection, typeof(Account.Account).Name);

			DropTable(sqlConnection, typeof(Byarbejde.ExternalByarbejde).Name);
			DropTable(sqlConnection, typeof(Byarbejde.Byarbejde).Name);

			DropTable(sqlConnection, typeof(Contact.Contact).Name);

			DropTable(sqlConnection, typeof(Group.Group).Name);

			DropTable(sqlConnection, typeof(ChangeProvider).Name);
		}

		public static void RecreateAllTables(SqlConnection sqlConnection)
		{
			DeleteAllTables(sqlConnection);
			MaintainAllTables(sqlConnection);
		}

		public static void DropTable(SqlConnection sqlConnection, string tableName)
		{
			Procedures.DropTable.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("DropTable");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("debug", 0));
		}

		public enum DataType
		{
			NVARCHAR_MAX = 1
			, INT = 2
			, DATETIME = 3
			, UNIQUEIDENTIFIER = 4
			, BIT = 5
		}

		private static string GetTypeString(DataType type)
		{
			switch (type)
			{
				case DataType.NVARCHAR_MAX:
					return "NVARCHAR(MAX)";
				case DataType.INT:
					return "INT";
				case DataType.DATETIME:
					return "DATETIME";
				case DataType.UNIQUEIDENTIFIER:
					return "UNIQUEIDENTIFIER";
				case DataType.BIT:
					return "BIT";
				default:
					throw new ArgumentException($"unknown datatype {type}");
			}
		}

		public static void AddColumn(SqlConnection sqlConnection, string tableName, string columnName, DataType type, SqlBoolean allowNull)
		{
			Procedures.AddColumn.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("AddColumn");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("columnName", columnName),
				new KeyValuePair<string, object>("type", GetTypeString(type)),
				new KeyValuePair<string, object>("allowNull", allowNull),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainForeignKey(SqlConnection sqlConnection, string tableName, string foreignKeyName, string primaryTablename, string primaryKeyName, bool cascade = true)
		{
			Procedures.MaintainForeignKey.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("MaintainForeignKey");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("foreignKeyName", foreignKeyName),
				new KeyValuePair<string, object>("primaryTablename", primaryTablename),
				new KeyValuePair<string, object>("primaryKeyName", primaryKeyName),
				new KeyValuePair<string, object>("cascade", cascade ? 1 : 0),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainCompositeForeignKey2Keys(SqlConnection sqlConnection, string tableName, string foreignKey1Name, string foreignKey2Name, string primaryTablename, string primaryKey1Name, string primaryKey2Name, bool cascade)
		{
			Procedures.MaintainCompositeForeignKey2Keys.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("MaintainCompositeForeignKey2Keys");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("foreignKey1Name", foreignKey1Name),
				new KeyValuePair<string, object>("foreignKey2Name", foreignKey2Name),
				new KeyValuePair<string, object>("primaryTablename", primaryTablename),
				new KeyValuePair<string, object>("primaryKey1Name", primaryKey1Name),
				new KeyValuePair<string, object>("primaryKey2Name", primaryKey2Name),
				new KeyValuePair<string, object>("cascade", cascade ? 1 : 0),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainCompositeForeignKey3Keys(SqlConnection sqlConnection, string tableName, string foreignKey1Name, string foreignKey2Name, string foreignKey3Name, string primaryTablename, string primaryKey1Name, string primaryKey2Name, string primaryKey3Name, bool cascade)
		{
			Procedures.MaintainCompositeForeignKey3Keys.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append("MaintainCompositeForeignKey3Keys");

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("foreignKey1Name", foreignKey1Name),
				new KeyValuePair<string, object>("foreignKey2Name", foreignKey2Name),
				new KeyValuePair<string, object>("foreignKey3Name", foreignKey3Name),
				new KeyValuePair<string, object>("primaryTablename", primaryTablename),
				new KeyValuePair<string, object>("primaryKey1Name", primaryKey1Name),
				new KeyValuePair<string, object>("primaryKey2Name", primaryKey2Name),
				new KeyValuePair<string, object>("primaryKey3Name", primaryKey3Name),
				new KeyValuePair<string, object>("cascade", cascade ? 1 : 0),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainUniqueConstraint(SqlConnection sqlConnection, string tableName, string constraintName, string constraintColumn1, string constraintColumn2)
		{
			Procedures.MaintainUniqueConstraint2Columns.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append(Procedures.MaintainUniqueConstraint2Columns.ProcedureName);

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("constraintName", constraintName),
				new KeyValuePair<string, object>("constraintColumn1", constraintColumn1),
				new KeyValuePair<string, object>("constraintColumn2", constraintColumn2),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static void MaintainUniqueConstraint(SqlConnection sqlConnection, string tableName, string constraintName, string constraintColumn)
		{
			Procedures.MaintainUniqueConstraint1Column.MakeSureProcedureExists(sqlConnection);

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.Append(Procedures.MaintainUniqueConstraint1Column.ProcedureName);

			ExecuteNonQuery(sqlConnection, sqlStringBuilder, CommandType.StoredProcedure,
				new KeyValuePair<string, object>("tableName", tableName),
				new KeyValuePair<string, object>("constraintName", constraintName),
				new KeyValuePair<string, object>("constraintColumn", constraintColumn),
				new KeyValuePair<string, object>("debug", 0));
		}

		public static DataType ReadNextById<DataType>(SqlConnection sqlConnection, Guid id, string[] columns, Func<DataRow, DataType> CreateFromRow)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT TOP 1");
			sqlStringBuilder.AppendLine("	id");
			foreach (string column in columns)
			{
				sqlStringBuilder.AppendLine($"	,{column}");
			}
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine("	" + typeof(DataType).Name);
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{typeof(DataType).Name}.id > @id");
			sqlStringBuilder.AppendLine("ORDER BY");
			sqlStringBuilder.AppendLine($"	{typeof(DataType).Name}.id");

			DataTable dataTable = ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>("id", id));

			if (dataTable.Rows.Count == 1)
			{
				DataRow row = dataTable.Rows[0];

				DataType dataObject = CreateFromRow(row);

				return dataObject;
			}

			if (id == Guid.Empty)
			{
				return default(DataType);
			}

			return ReadNextById(sqlConnection, Guid.Empty, columns, CreateFromRow);
		}

		internal static List<Guid> ReadNNTable(SqlConnection sqlConnection, Type NNTableDataType, string NNTableSearchId, string NNTableRelationId, Guid searchIdValue)
		{
			string tableName = NNTableDataType.Name;

			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder.AppendLine("SELECT");
			sqlStringBuilder.AppendLine($"	[{tableName}].{NNTableRelationId}");
			sqlStringBuilder.AppendLine("FROM");
			sqlStringBuilder.AppendLine($"	[{tableName}]");
			sqlStringBuilder.AppendLine("WHERE");
			sqlStringBuilder.AppendLine($"	{tableName}.{NNTableSearchId} = @{NNTableSearchId}");

			DataTable dataTable = ExecuteAdapterSelect(sqlConnection, sqlStringBuilder, new KeyValuePair<string, object>(NNTableSearchId, searchIdValue));

			List<Guid> relatedIds = new List<Guid>();

			foreach (DataRow row in dataTable.Rows)
			{
				Guid id = (Guid)row[NNTableRelationId];

				relatedIds.Add(id);
			}

			return relatedIds;
		}

		public static List<SqlColumnInfo> GetSqlColumnsInfo(Type holderType)
		{
			List<SqlColumnInfo> dataColumns = new List<SqlColumnInfo>();

			IEnumerable<FieldInfo> fieldsInfo = holderType.GetFields(BindingFlags.Public | BindingFlags.Instance).
				Where(field => field.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(SqlColumn)));

			IEnumerable<PropertyInfo> propertiesInfo = holderType.GetProperties(BindingFlags.Public | BindingFlags.Instance).
				Where(field => field.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(SqlColumn)));

			dataColumns.AddRange(fieldsInfo.Select(info => GetSqlColumnInfoFromMemberInfo(info)));

			dataColumns.AddRange(propertiesInfo.Select(info => GetSqlColumnInfoFromMemberInfo(info)));

			return dataColumns;
		}

		private static SqlColumnInfo GetSqlColumnInfoFromMemberInfo(MemberInfo info)
		{
			CustomAttributeData SqlFieldAttributeData = info.CustomAttributes.Single(attribute => attribute.AttributeType == typeof(SqlColumn));

			object[] arguments = SqlFieldAttributeData.ConstructorArguments.Select(data =>
			{
				System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument> collection;

				switch (data.ArgumentType.Name)
				{
					case "Boolean[]":
						collection = (System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)data.Value;
						return collection.Select(value => (bool)value.Value).ToArray();
					case "String[]":
						collection = (System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)data.Value;
						return collection.Select(value => (string)value.Value).ToArray();
					case "Int32[]":
						collection = (System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)data.Value;
						return collection.Select(value => (int)value.Value).ToArray();
					case "Type[]":
						collection = (System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)data.Value;
						return collection.Select(value => (Type)value.Value).ToArray();
					default:
						return data.Value;
				}
			}).ToArray();

			SqlColumn sqlColumn = (SqlColumn)SqlFieldAttributeData.Constructor.Invoke(arguments);

			return new SqlColumnInfo(info.Name, sqlColumn);
		}

		public static void MaintainTable(SqlConnection sqlConnection, Type SqlDataType)
		{
			string tableName = SqlDataType.Name;

			List<string> columnsInDatabase = GetExistingColumns(sqlConnection, tableName);

			List<SqlColumnInfo> allColumns = GetSqlColumnsInfo(SqlDataType);

			MakeSureTableExists(sqlConnection, tableName, columnsInDatabase, allColumns);

			MakeSureColumnsExists(sqlConnection, tableName, columnsInDatabase, allColumns);

			MakeSureForeignKeysExists(sqlConnection, tableName, allColumns);
		}

		private static void MakeSureForeignKeysExists(SqlConnection sqlConnection, string tableName, List<SqlColumnInfo> allColumns)
		{
			List<SqlColumnInfo> foreignKeyFields = allColumns.Where(SqlColumnInfo.IsForeignKey).ToList();

			foreignKeyFields.ForEach(keyField => keyField.SqlColumn.ForeignKeysInfo.ForEach(info => info.SqlColumnInfo = keyField));

			List<ForeignKeyInfo> keyInfo = foreignKeyFields.SelectMany(key => key.SqlColumn.ForeignKeysInfo).ToList();

			var keyGroups = keyInfo.GroupBy(key => new { type = key.ForeignKeyTable, group = key.ForeignKeyGroup });

			foreach (var keyGroup in keyGroups)
			{
				var orderedKeyGroup = keyGroup.OrderBy(key => key.ForeignKeyIndex);
				switch (keyGroup.Count())
				{
					case 1:
						ForeignKeyInfo info = keyGroup.Single();
						AbstractData.CreateKeyIfMissing(sqlConnection, tableName, info.SqlColumnInfo.Name.ToLower(), info.ForeignKeyTable.Name, info.ForeignKeyIdName, info.ForeignKeyCascade);
						break;
					case 2:
						{
							string foreignKey1Name = orderedKeyGroup.ElementAt(0).SqlColumnInfo.Name.ToLower();
							string foreignKey2Name = orderedKeyGroup.ElementAt(1).SqlColumnInfo.Name.ToLower();
							Type foreignTable = keyGroup.First().ForeignKeyTable;
							string primaryKey1Name = orderedKeyGroup.ElementAt(0).ForeignKeyIdName;
							string primaryKey2Name = orderedKeyGroup.ElementAt(1).ForeignKeyIdName;
							bool cascade = keyGroup.First().ForeignKeyCascade;
							MaintainCompositeForeignKey2Keys(sqlConnection, tableName, foreignKey1Name, foreignKey2Name, foreignTable.Name, primaryKey1Name, primaryKey2Name, cascade);
						}
						break;
					case 3:
						{
							string foreignKey1Name = orderedKeyGroup.ElementAt(0).SqlColumnInfo.Name.ToLower();
							string foreignKey2Name = orderedKeyGroup.ElementAt(1).SqlColumnInfo.Name.ToLower();
							string foreignKey3Name = orderedKeyGroup.ElementAt(2).SqlColumnInfo.Name.ToLower();
							Type foreignTable = orderedKeyGroup.First().ForeignKeyTable;
							string primaryKey1Name = orderedKeyGroup.ElementAt(0).ForeignKeyIdName;
							string primaryKey2Name = orderedKeyGroup.ElementAt(1).ForeignKeyIdName;
							string primaryKey3Name = orderedKeyGroup.ElementAt(2).ForeignKeyIdName;
							bool cascade = orderedKeyGroup.First().ForeignKeyCascade;
							MaintainCompositeForeignKey3Keys(sqlConnection, tableName, foreignKey1Name, foreignKey2Name, foreignKey3Name, foreignTable.Name, primaryKey1Name, primaryKey2Name, primaryKey3Name, cascade);
						}
						break;
					default:
						throw new Exception($"{keyGroup.Count()} foreign keys not supported for group {keyGroup.Key.group} on type {keyGroup.Key.type.Name}");
				}
			}
		}

		private static void MakeSureColumnsExists(SqlConnection sqlConnection, string tableName, List<string> columnsInDatabase, List<SqlColumnInfo> allColumns)
		{
			List<SqlColumnInfo> nonPrimaryKeyFields = allColumns.Where(SqlColumnInfo.IsNotPrimaryKey).ToList();

			foreach (SqlColumnInfo sqlColumnInfo in nonPrimaryKeyFields)
			{
				AbstractData.CreateIfMissing(sqlConnection, tableName, columnsInDatabase, sqlColumnInfo.Name.ToLower(), sqlColumnInfo.SqlColumn.DataType, sqlColumnInfo.SqlColumn.AllowNull);
			}
		}

		private static void MakeSureTableExists(SqlConnection sqlConnection, string tableName, List<string> columnsInDatabase, List<SqlColumnInfo> allColumns)
		{
			if (columnsInDatabase.Any() == false)
			{
				List<string> keyNames = allColumns.Where(SqlColumnInfo.IsPrimaryKey).Select(column => column.Name.ToLower()).ToList();
				int primaryKeyCount = keyNames.Count();

				switch (primaryKeyCount)
				{
					case 1:
						CreateTable(sqlConnection, tableName, keyNames.Single());
						break;
					case 2:
						CreateCompositeTable2Tables(sqlConnection, tableName, keyNames[0], keyNames[1]);
						break;
					case 3:
						CreateCompositeTable3Tables(sqlConnection, tableName, keyNames[0], keyNames[1], keyNames[2]);
						break;
					default:
						throw new Exception($"tables with {primaryKeyCount} keys are not supported");
				}
			}
		}
	}
}