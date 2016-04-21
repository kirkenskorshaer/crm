using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer.SqlData.Annotation
{
	public class ExternalAccountAnnotation : AbstractData
	{
		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
		public Guid ExternalAnnotationId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey | SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "accountAnnotation", typeof(AccountAnnotation), "id", true, 1)]
		public Guid AccountAnnotationId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey | SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "changeprovider", typeof(ChangeProvider), "id", true, 1)]
		public Guid ChangeProviderId { get; private set; }

		public ExternalAccountAnnotation()
		{
		}

		public ExternalAccountAnnotation(Guid externalAnnotationId, Guid changeProviderId, Guid accountAnnotationId)
		{
			ExternalAnnotationId = externalAnnotationId;
			ChangeProviderId = changeProviderId;
			AccountAnnotationId = accountAnnotationId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(ExternalAccountAnnotation);

			SqlUtilities.MaintainTable(sqlConnection, dataClassType);
		}

		public static List<ExternalAccountAnnotation> ReadFromChangeProviderAndAnnotation(SqlConnection sqlConnection, Guid changeProviderId, Guid accountAnnotationId)
		{
			return Read<ExternalAccountAnnotation>(sqlConnection, new List<SqlCondition> { new SqlCondition("ChangeProviderId", "=", changeProviderId), new SqlCondition("AccountAnnotationId", "=", accountAnnotationId) });
		}

		public static List<ExternalAccountAnnotation> ReadFromChangeProviderAndExternalAnnotation(SqlConnection sqlConnection, Guid changeProviderId, Guid ExternalAnnotationId)
		{
			return Read<ExternalAccountAnnotation>(sqlConnection, new List<SqlCondition> { new SqlCondition("ChangeProviderId", "=", changeProviderId), new SqlCondition("ExternalAnnotationId", "=", ExternalAnnotationId) });
		}
	}
}
