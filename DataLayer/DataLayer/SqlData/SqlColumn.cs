using System;

namespace DataLayer.SqlData
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class SqlColumn : Attribute
	{
		public enum PropertyEnum
		{
			None = 0,
			PrimaryKey = 1,
			ForeignKey = 2,
		}

		public SqlColumn(PropertyEnum properties, Utilities.DataType dataType, bool allowNull, Type foreignKeyTable = null, string foreignKeyIdName = null, bool foreignKeyCascade = false)
		{
			Properties = properties;
			DataType = dataType;
			AllowNull = allowNull;
			ForeignKeyTable = foreignKeyTable;
			ForeignKeyIdName = foreignKeyIdName;
			ForeignKeyCascade = foreignKeyCascade;
		}

		public PropertyEnum Properties { get; private set; }
		public Utilities.DataType DataType { get; private set; }
		public bool AllowNull { get; private set; }
		public Type ForeignKeyTable { get; private set; }
		public string ForeignKeyIdName { get; private set; }
		public bool ForeignKeyCascade { get; private set; }
	}
}
