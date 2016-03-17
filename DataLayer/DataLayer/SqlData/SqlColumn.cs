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
		}

		public SqlColumn(PropertyEnum properties, Utilities.DataType dataType, bool allowNull)
		{
			Properties = properties;
			DataType = dataType;
			AllowNull = allowNull;
		}

		public PropertyEnum Properties { get; private set; }
		public Utilities.DataType DataType { get; private set; }
		public bool AllowNull { get; private set; }
	}
}
