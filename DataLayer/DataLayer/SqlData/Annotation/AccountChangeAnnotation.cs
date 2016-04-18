using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Annotation
{
	public class AccountChangeAnnotation : AbstractIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, false)]
		public string notetext;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "accountchangeid", typeof(Account.AccountChange), "id", true, 1)]
		public Guid AccountChangeId;

		public AccountChangeAnnotation()
		{
		}

		public AccountChangeAnnotation(Guid accountChangeId)
		{
			AccountChangeId = accountChangeId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(AccountChangeAnnotation);

			SqlUtilities.MaintainTable(sqlConnection, dataClassType);
		}

		public static AccountChangeAnnotation Read(SqlConnection sqlConnection, Guid id)
		{
			List<AccountChangeAnnotation> annotations = Read<AccountChangeAnnotation>(sqlConnection, "id", id);

			return annotations.Single();
		}

		public static bool ExistsById(SqlConnection sqlConnection, Guid id)
		{
			return Exists<AccountChangeAnnotation>(sqlConnection, "id", id);
		}
	}
}
