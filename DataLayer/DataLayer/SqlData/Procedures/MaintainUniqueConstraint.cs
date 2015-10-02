using System.Data.SqlClient;
using System.Text;

namespace DataLayer.SqlData.Procedures
{
	public static class MaintainUniqueConstraint
	{
		internal static void MakeSureProcedureExists(SqlConnection sqlConnection)
		{
			DeleteProcedure(sqlConnection);

			CreateProcedure(sqlConnection);
		}

		private static void DeleteProcedure(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("IF");
			sqlStringBuilder.AppendLine("(");
			sqlStringBuilder.AppendLine("	EXISTS");
			sqlStringBuilder.AppendLine("	(");
			sqlStringBuilder.AppendLine("		SELECT");
			sqlStringBuilder.AppendLine("			*");
			sqlStringBuilder.AppendLine("		FROM");
			sqlStringBuilder.AppendLine("			SYS.objects");
			sqlStringBuilder.AppendLine("		WHERE");
			sqlStringBuilder.AppendLine("			objects.type = 'P'");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			objects.name = 'MaintainUniqueConstraint'");
			sqlStringBuilder.AppendLine("	)");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DROP PROCEDURE MaintainUniqueConstraint");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}

		private static void CreateProcedure(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("CREATE PROCEDURE");
			sqlStringBuilder.AppendLine("	MaintainUniqueConstraint");
			sqlStringBuilder.AppendLine("	@tablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@constraintName NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@constraintColumn1 NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@constraintColumn2 NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@debug BIT = 1");
			sqlStringBuilder.AppendLine("AS");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	IF(NOT EXISTS");
			sqlStringBuilder.AppendLine("	(");
			sqlStringBuilder.AppendLine("		SELECT");
			sqlStringBuilder.AppendLine("			objects.name [ConstraintTable]");
			sqlStringBuilder.AppendLine("			,ConstrainObjects.name [ConstraintName]");
			sqlStringBuilder.AppendLine("			");
			sqlStringBuilder.AppendLine("		FROM");
			sqlStringBuilder.AppendLine("			sys.objects");
			sqlStringBuilder.AppendLine("		JOIN");
			sqlStringBuilder.AppendLine("			sys.objects ConstrainObjects");
			sqlStringBuilder.AppendLine("		ON");
			sqlStringBuilder.AppendLine("			objects.object_id = ConstrainObjects.parent_object_id");
			sqlStringBuilder.AppendLine("		WHERE");
			sqlStringBuilder.AppendLine("			objects.[type] = 'U'");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			objects.name = @tablename");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			ConstrainObjects.[type] = 'UQ'");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			ConstrainObjects.name = @constraintName");
			sqlStringBuilder.AppendLine("	))");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		DECLARE @sql nvarchar(4000)");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		SET @sql = '");
			sqlStringBuilder.AppendLine("		ALTER TABLE");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@tablename) + '");
			sqlStringBuilder.AppendLine("		ADD CONSTRAINT");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@constraintName) + '");
			sqlStringBuilder.AppendLine("		UNIQUE");
			sqlStringBuilder.AppendLine("		(");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@constraintColumn1) + '");
			sqlStringBuilder.AppendLine("			,' + QUOTENAME(@constraintColumn2) + '");
			sqlStringBuilder.AppendLine("		)'");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		IF(@debug = 1)");
			sqlStringBuilder.AppendLine("		BEGIN");
			sqlStringBuilder.AppendLine("			PRINT @sql");
			sqlStringBuilder.AppendLine("		END");
			sqlStringBuilder.AppendLine("		ELSE");
			sqlStringBuilder.AppendLine("		BEGIN");
			sqlStringBuilder.AppendLine("			EXEC SP_EXECUTESQL @sql, N'@tablename NVARCHAR(128), @constraintName NVARCHAR(128), @constraintColumn1 NVARCHAR(128), @constraintColumn2 NVARCHAR(128)', @tablename = @tablename, @constraintName = @constraintName, @constraintColumn1 = @constraintColumn1, @constraintColumn2 = @constraintColumn2");
			sqlStringBuilder.AppendLine("		END");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		IF(@debug = 1)");
			sqlStringBuilder.AppendLine("		BEGIN");
			sqlStringBuilder.AppendLine("			PRINT 'unique constraint already exists'");
			sqlStringBuilder.AppendLine("		END");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}
	}
}
