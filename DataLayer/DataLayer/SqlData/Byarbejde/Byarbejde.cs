using System;
using System.Data.SqlClient;

namespace DataLayer.SqlData.Byarbejde
{
	public class Byarbejde : AbstractIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, Utilities.DataType.NVARCHAR_MAX, false)]
		public string new_name;

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(Byarbejde);

			Utilities.MaintainTable(sqlConnection, dataClassType);
		}
	}
}
