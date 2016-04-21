using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer.SqlData.Annotation
{
	public class ExternalContactAnnotation : AbstractData
	{
		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
		public Guid ExternalAnnotationId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey | SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "contactAnnotation", typeof(ContactAnnotation), "id", true, 1)]
		public Guid ContactAnnotationId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey | SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "changeprovider", typeof(ChangeProvider), "id", true, 1)]
		public Guid ChangeProviderId { get; private set; }

		public ExternalContactAnnotation()
		{
		}

		public ExternalContactAnnotation(Guid externalAnnotationId, Guid changeProviderId, Guid contactAnnotationId)
		{
			ExternalAnnotationId = externalAnnotationId;
			ChangeProviderId = changeProviderId;
			ContactAnnotationId = contactAnnotationId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(ExternalContactAnnotation);

			SqlUtilities.MaintainTable(sqlConnection, dataClassType);
		}

		public static List<ExternalContactAnnotation> ReadFromChangeProviderAndAnnotation(SqlConnection sqlConnection, Guid changeProviderId, Guid contactAnnotationId)
		{
			return Read<ExternalContactAnnotation>(sqlConnection, new List<SqlCondition> { new SqlCondition("ChangeProviderId", "=", changeProviderId), new SqlCondition("ContactAnnotationId", "=", contactAnnotationId) });
		}

		public static List<ExternalContactAnnotation> ReadFromChangeProviderAndExternalAnnotation(SqlConnection sqlConnection, Guid changeProviderId, Guid ExternalAnnotationId)
		{
			return Read<ExternalContactAnnotation>(sqlConnection, new List<SqlCondition> { new SqlCondition("ChangeProviderId", "=", changeProviderId), new SqlCondition("ExternalAnnotationId", "=", ExternalAnnotationId) });
		}
	}
}
