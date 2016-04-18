using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Annotation
{
	public class AccountAnnotation : AbstractIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, false)]
		public string notetext;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "accountid", typeof(Account.Account), "id", true, 1)]
		public Guid AccountId;

		public AccountAnnotation()
		{
		}

		public AccountAnnotation(Guid accountId)
		{
			AccountId = accountId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(AccountAnnotation);

			SqlUtilities.MaintainTable(sqlConnection, dataClassType);
		}

		public static AccountAnnotation Read(SqlConnection sqlConnection, Guid id)
		{
			List<AccountAnnotation> annotations = Read<AccountAnnotation>(sqlConnection, "id", id);

			return annotations.Single();
		}

		public static bool ExistsById(SqlConnection sqlConnection, Guid id)
		{
			return Exists<AccountAnnotation>(sqlConnection, "id", id);
		}

		public static List<AccountAnnotation> ReadByAccountId(SqlConnection sqlConnection, Guid accountId)
		{
			return Read<AccountAnnotation>(sqlConnection, new List<SqlCondition>()
			{
				new SqlCondition("AccountId", "=", accountId),
			});
		}
	}
}
