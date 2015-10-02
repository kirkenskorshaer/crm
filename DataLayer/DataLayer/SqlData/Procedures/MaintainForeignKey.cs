using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.SqlData.Procedures
{
	internal static class MaintainForeignKey
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
			sqlStringBuilder.AppendLine("			objects.name = 'MaintainForeignKey'");
			sqlStringBuilder.AppendLine("	)");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DROP PROCEDURE MaintainForeignKey");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}

		private static void CreateProcedure(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("CREATE PROCEDURE");
			sqlStringBuilder.AppendLine("	MaintainForeignKey");
			sqlStringBuilder.AppendLine("	@tablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@foreignKeyName NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryTablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryKeyName NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@debug BIT = 1");
			sqlStringBuilder.AppendLine("AS");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	IF(NOT EXISTS");
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
			sqlStringBuilder.AppendLine("			columns.name = @foreignKeyName");
			sqlStringBuilder.AppendLine("			AND");
			sqlStringBuilder.AppendLine("			childColumns.name = @primaryKeyName");
			sqlStringBuilder.AppendLine("	))");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		DECLARE @sql nvarchar(4000)");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("		SET @sql = '");
			sqlStringBuilder.AppendLine("		ALTER TABLE");
			sqlStringBuilder.AppendLine("			' + QUOTENAME(@tablename) + '");
			sqlStringBuilder.AppendLine("		ADD FOREIGN KEY");
			sqlStringBuilder.AppendLine("		(' + QUOTENAME(@foreignKeyName) + ')");
			sqlStringBuilder.AppendLine("		REFERENCES'+");
			sqlStringBuilder.AppendLine("			QUOTENAME(@primaryTablename) + '");
			sqlStringBuilder.AppendLine("		(' + QUOTENAME(@primaryKeyName) + ')");
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
			sqlStringBuilder.AppendLine("			EXEC SP_EXECUTESQL @sql, N'@tablename NVARCHAR(128), @foreignKeyName NVARCHAR(128), @primaryTablename NVARCHAR(128), @primaryKeyName NVARCHAR(128)', @tablename = @tablename, @foreignKeyName = @foreignKeyName, @primaryTablename = @primaryTablename, @primaryKeyName = @primaryKeyName");
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
