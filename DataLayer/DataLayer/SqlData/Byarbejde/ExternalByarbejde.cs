using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer.SqlData.Byarbejde
{
	public class ExternalByarbejde : AbstractData
	{
		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false)]
		public Guid ExternalByarbejdeId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey | SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "byarbejde", typeof(Byarbejde), "id", true, 1)]
		public Guid ByarbejdeId { get; private set; }

		[SqlColumn(SqlColumn.PropertyEnum.PrimaryKey | SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "changeprovider", typeof(ChangeProvider), "id", true, 1)]
		public Guid ChangeProviderId { get; private set; }

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			ChangeProvider.MaintainTable(sqlConnection);
			Byarbejde.MaintainTable(sqlConnection);

			SqlUtilities.MaintainTable(sqlConnection, typeof(ExternalByarbejde));
		}

		public ExternalByarbejde()
		{
		}

		public ExternalByarbejde(Guid externalByarbejdeId, Guid changeProviderId, Guid byarbejdeId)
		{
			ExternalByarbejdeId = externalByarbejdeId;
			ChangeProviderId = changeProviderId;
			ByarbejdeId = byarbejdeId;
        }

		public static List<ExternalByarbejde> ReadFromChangeProviderAndByarbejde(SqlConnection sqlConnection, Guid changeProviderId, Guid byarbejdeId)
		{
			return Read<ExternalByarbejde>(sqlConnection, new List<SqlCondition> { new SqlCondition("ChangeProviderId", "=", changeProviderId), new SqlCondition("ByarbejdeId", "=", byarbejdeId) });
		}

		public static List<ExternalByarbejde> ReadFromChangeProviderAndExternalByarbejde(SqlConnection sqlConnection, Guid changeProviderId, Guid ExternalByarbejdeId)
		{
			return Read<ExternalByarbejde>(sqlConnection, new List<SqlCondition> { new SqlCondition("ChangeProviderId", "=", changeProviderId), new SqlCondition("ExternalByarbejdeId", "=", ExternalByarbejdeId) });
		}
	}
}
