using System;

namespace DataLayer.SqlData
{
	public class ForeignKeyInfo
	{
		public ForeignKeyInfo(string foreignKeyGroup, Type foreignKeyTable, string foreignKeyIdName, bool foreignKeyCascade, int foreignKeyIndex)
		{
			ForeignKeyGroup = foreignKeyGroup;
			ForeignKeyTable = foreignKeyTable;
			ForeignKeyIdName = foreignKeyIdName;
			ForeignKeyCascade = foreignKeyCascade;
			ForeignKeyIndex = foreignKeyIndex;
        }

		public string ForeignKeyGroup { get; private set; }
		public Type ForeignKeyTable { get; private set; }
		public string ForeignKeyIdName { get; private set; }
		public bool ForeignKeyCascade { get; private set; }
		public int ForeignKeyIndex { get; private set; }

		public SqlColumnInfo SqlColumnInfo { get; set; }
	}
}
