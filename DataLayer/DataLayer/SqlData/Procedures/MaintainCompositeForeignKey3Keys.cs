using System.Data.SqlClient;
using System.Text;

namespace DataLayer.SqlData.Procedures
{
	internal static class MaintainCompositeForeignKey3Keys
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
			sqlStringBuilder.AppendLine("			objects.name = 'MaintainCompositeForeignKey3Keys'");
			sqlStringBuilder.AppendLine("	)");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DROP PROCEDURE MaintainCompositeForeignKey3Keys");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}

		private static void CreateProcedure(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("CREATE PROCEDURE");
			sqlStringBuilder.AppendLine("	MaintainCompositeForeignKey3Keys");
			sqlStringBuilder.AppendLine("	@tablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@foreignKey1Name NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@foreignKey2Name NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@foreignKey3Name NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryTablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryKey1Name NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryKey2Name NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryKey3Name NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@debug BIT = 1");
			sqlStringBuilder.AppendLine("AS");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("IF(");
			sqlStringBuilder.AppendLine("	SELECT");
			sqlStringBuilder.AppendLine("		COUNT(*)");
			sqlStringBuilder.AppendLine("	FROM");
			sqlStringBuilder.AppendLine("	(");
			sqlStringBuilder.AppendLine("		SELECT");
			sqlStringBuilder.AppendLine("			objects.name [ForeignKeyTable]");
			sqlStringBuilder.AppendLine("			,columns.name [ForeignKey]");
			sqlStringBuilder.AppendLine("			,childColumns.name [PrimaryKey]");
			sqlStringBuilder.AppendLine("		FROM");
			sqlStringBuilder.AppendLine("			sys.objects");
			sqlStringBuilder.AppendLine("		JOIN");
			sqlStringBuilder.AppendLine("			sys.foreign_keys");
			sqlStringBuilder.AppendLine("		ON");
			sqlStringBuilder.AppendLine("			foreign_keys.parent_object_id = objects.object_id");
			sqlStringBuilder.AppendLine("		JOIN");
			sqlStringBuilder.AppendLine("			sys.foreign_key_columns");
			sqlStringBuilder.AppendLine("		ON");
			sqlStringBuilder.AppendLine("			foreign_key_columns.constraint_object_id = foreign_keys.object_id");
			sqlStringBuilder.AppendLine("		JOIN");
			sqlStringBuilder.AppendLine("			sys.columns");
			sqlStringBuilder.AppendLine("		ON");
			sqlStringBuilder.AppendLine("			columns.object_id = objects.object_id");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			columns.column_id = foreign_key_columns.parent_column_id");
			sqlStringBuilder.AppendLine("		JOIN");
			sqlStringBuilder.AppendLine("			sys.columns childColumns");
			sqlStringBuilder.AppendLine("		ON");
			sqlStringBuilder.AppendLine("			childColumns.object_id = foreign_keys.referenced_object_id");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			childColumns.column_id = foreign_key_columns.referenced_column_id");
			sqlStringBuilder.AppendLine("		WHERE");
			sqlStringBuilder.AppendLine("			objects.[type] = 'U'");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			objects.name = @tablename");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			foreign_keys.delete_referential_action = 1");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			foreign_keys.update_referential_action = 1");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			columns.name IN (@foreignKey1Name,@foreignKey2Name,@foreignKey3Name)");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			childColumns.name IN (@primaryKey1Name,@primaryKey2Name,@primaryKey3Name)");
			sqlStringBuilder.AppendLine("		) foreignKeys ) != 3");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		DECLARE @sql nvarchar(4000)");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		SET @sql = '");
			sqlStringBuilder.AppendLine("		ALTER TABLE");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@tablename) + '");
			sqlStringBuilder.AppendLine("		ADD FOREIGN KEY");
			sqlStringBuilder.AppendLine("		(");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@foreignKey1Name) + '");
			sqlStringBuilder.AppendLine("			,' + QUOTENAME(@foreignKey2Name) + '");
			sqlStringBuilder.AppendLine("			,' + QUOTENAME(@foreignKey3Name) + '");
			sqlStringBuilder.AppendLine("		)");
			sqlStringBuilder.AppendLine("		REFERENCES'+");
			sqlStringBuilder.AppendLine("			QUOTENAME(@primaryTablename) + '");
			sqlStringBuilder.AppendLine("		(");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@primaryKey1Name) + '");
			sqlStringBuilder.AppendLine("			,' + QUOTENAME(@primaryKey2Name) + '");
			sqlStringBuilder.AppendLine("			,' + QUOTENAME(@primaryKey3Name) + '");
			sqlStringBuilder.AppendLine("		)");
			sqlStringBuilder.AppendLine("		ON DELETE");
			sqlStringBuilder.AppendLine("			CASCADE");
			sqlStringBuilder.AppendLine("		ON UPDATE");
			sqlStringBuilder.AppendLine("			CASCADE'");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		IF(@debug = 1)");
			sqlStringBuilder.AppendLine("		BEGIN");
			sqlStringBuilder.AppendLine("			PRINT @sql");
			sqlStringBuilder.AppendLine("		END");
			sqlStringBuilder.AppendLine("		ELSE");
			sqlStringBuilder.AppendLine("		BEGIN");
			sqlStringBuilder.AppendLine("			EXEC SP_EXECUTESQL @sql, N'@tablename NVARCHAR(128), @foreignKey1Name NVARCHAR(128), @foreignKey2Name NVARCHAR(128), @foreignKey3Name NVARCHAR(128), @primaryTablename NVARCHAR(128), @primaryKey1Name NVARCHAR(128), @primaryKey2Name NVARCHAR(128), @primaryKey3Name NVARCHAR(128)', @tablename = @tablename, @foreignKey1Name = @foreignKey1Name, @foreignKey2Name = @foreignKey2Name, @foreignKey3Name = @foreignKey3Name, @primaryTablename = @primaryTablename, @primaryKey1Name = @primaryKey1Name, @primaryKey2Name = @primaryKey2Name, @primaryKey3Name = @primaryKey3Name");
			sqlStringBuilder.AppendLine("		END");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		IF(@debug = 1)");
			sqlStringBuilder.AppendLine("		BEGIN");
			sqlStringBuilder.AppendLine("			PRINT 'key already exists'");
			sqlStringBuilder.AppendLine("		END");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}
	}
}
