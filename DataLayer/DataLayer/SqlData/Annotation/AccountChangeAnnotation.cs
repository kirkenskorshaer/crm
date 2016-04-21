using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Annotation
{
	public class AccountChangeAnnotation : AbstractIdData, IDeletableModifiedIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, false)]
		public string notetext;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "accountchangeid", typeof(Account.AccountChange), "id", true, 1)]
		public Guid AccountChangeId;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "AccountAnnotationId", typeof(AccountAnnotation), "id", false, 1)]
		public Guid AccountAnnotationId;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.DATETIME, false)]
		public DateTime modifiedon { get; set; }

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.BIT, false)]
		public bool isdeleted { get; set; }

		public AccountChangeAnnotation()
		{
		}

		public AccountChangeAnnotation(Guid accountChangeId, Guid accountAnnotationId)
		{
			AccountChangeId = accountChangeId;
			AccountAnnotationId = accountAnnotationId;
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
