using System;
using System.Collections.Generic;

namespace DataLayer.SqlData
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class SqlColumn : Attribute
	{
		public PropertyEnum Properties { get; private set; }
		public Utilities.DataType DataType { get; private set; }
		public bool AllowNull { get; private set; }
		public List<ForeignKeyInfo> ForeignKeysInfo { get; private set; }

		public enum PropertyEnum
		{
			None = 0,
			PrimaryKey = 1,
			ForeignKey = 2,
		}

		public SqlColumn(PropertyEnum properties, Utilities.DataType dataType, bool allowNull)
		{
			Properties = properties;
			DataType = dataType;
			AllowNull = allowNull;
		}

		public SqlColumn(PropertyEnum properties, Utilities.DataType dataType, bool allowNull, string foreignKeyGroup, Type foreignKeyTable, string foreignKeyIdName, bool foreignKeyCascade, int foreignKeyIndex)
		{
			Properties = properties;
			DataType = dataType;
			AllowNull = allowNull;
			ForeignKeyInfo info = new ForeignKeyInfo(foreignKeyGroup, foreignKeyTable, foreignKeyIdName, foreignKeyCascade, foreignKeyIndex);
			ForeignKeysInfo = new List<ForeignKeyInfo>() { info };
		}

		public SqlColumn(PropertyEnum properties, Utilities.DataType dataType, bool allowNull, string[] foreignKeyGroups, Type[] foreignKeyTables, string[] foreignKeyIdNames, bool[] foreignKeyCascades, int[] foreignKeyIndex)
		{
			Properties = properties;
			DataType = dataType;
			AllowNull = allowNull;

			ValidateArguments(foreignKeyGroups, foreignKeyTables, foreignKeyIdNames, foreignKeyCascades);

			ForeignKeysInfo = new List<ForeignKeyInfo>();

			for (int index = 0; index < foreignKeyGroups.Length; index++)
			{
				ForeignKeyInfo info = new ForeignKeyInfo(foreignKeyGroups[index], foreignKeyTables[index], foreignKeyIdNames[index], foreignKeyCascades[index], foreignKeyIndex[index]);
				ForeignKeysInfo.Add(info);
			}
		}

		private void ValidateArguments(string[] foreignKeyGroups, Type[] foreignKeyTables, string[] foreignKeyIdNames, bool[] foreignKeyCascades)
		{
			if (
				foreignKeyGroups.Length != foreignKeyTables.Length ||
				foreignKeyIdNames.Length != foreignKeyCascades.Length ||
				foreignKeyGroups.Length != foreignKeyCascades.Length)
			{
				throw new ArgumentException("Not all ForeignKey parameters are filled");
			}
		}

	}
}
