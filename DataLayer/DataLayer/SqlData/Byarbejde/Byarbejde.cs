using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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

		public static Byarbejde Read(SqlConnection sqlConnection, Guid id)
		{
			List<Byarbejde> byarbejder = Read<Byarbejde>(sqlConnection, "id", id);

			return byarbejder.Single();
		}

		public static Byarbejde ReadByName(SqlConnection sqlConnection, string name)
		{
			List<Byarbejde> byarbejder = Read<Byarbejde>(sqlConnection, "new_name", name);

			return byarbejder.Single();
		}

		public static bool ExistsByName(SqlConnection sqlConnection, string name)
		{
			return Exists<Byarbejde>(sqlConnection, "new_name", name);
		}

		public static bool ExistsById(SqlConnection sqlConnection, Guid id)
		{
			return Exists<Byarbejde>(sqlConnection, "id", id);
		}

		public static Byarbejde ReadByNameOrCreate(SqlConnection sqlConnection, string name)
		{
			if (ExistsByName(sqlConnection, name))
			{
				return ReadByName(sqlConnection, name);
			}

			Byarbejde byarbejde = new Byarbejde()
			{
				new_name = name,
			};

			byarbejde.Insert(sqlConnection);

			return byarbejde;
		}
	}
}
