namespace DataLayer.SqlData
{
	public class SqlColumnInfo
	{
		public SqlColumnInfo(string name, SqlColumn sqlField)
		{
			Name = name;
			SqlColumn = sqlField;
		}

		public string Name;
		public SqlColumn SqlColumn;

		public static bool IsPrimaryKey(SqlColumnInfo info)
		{
			return info.SqlColumn.Properties.HasFlag(SqlColumn.PropertyEnum.PrimaryKey);
		}

		public static bool IsNotPrimaryKey(SqlColumnInfo info)
		{
			return info.SqlColumn.Properties.HasFlag(SqlColumn.PropertyEnum.PrimaryKey) == false;
		}

		public static bool IsForeignKey(SqlColumnInfo info)
		{
			return info.SqlColumn.Properties.HasFlag(SqlColumn.PropertyEnum.ForeignKey);
		}
	}
}
