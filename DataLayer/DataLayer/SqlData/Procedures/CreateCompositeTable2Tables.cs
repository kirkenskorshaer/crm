﻿using System.Data.SqlClient;
using System.Text;

namespace DataLayer.SqlData.Procedures
{
	internal static class CreateCompositeTable2Tables
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
			sqlStringBuilder.AppendLine("			objects.name = 'CreateCompositeTable2Tables'");
			sqlStringBuilder.AppendLine("	)");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DROP PROCEDURE CreateCompositeTable2Tables");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}

		private static void CreateProcedure(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("CREATE PROCEDURE");
			sqlStringBuilder.AppendLine("	CreateCompositeTable2Tables");
			sqlStringBuilder.AppendLine("	@tablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryKeyName1 NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@primaryKeyName2 NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@debug BIT = 1");
			sqlStringBuilder.AppendLine("AS");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DECLARE @sql nvarchar(4000)");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("	SET @sql = '");
			sqlStringBuilder.AppendLine("	CREATE TABLE");
			sqlStringBuilder.AppendLine("		dbo.'+ QUOTENAME(@tablename) + '");
			sqlStringBuilder.AppendLine("	(");
			sqlStringBuilder.AppendLine("		'+QUOTENAME(@primaryKeyName1)+' UNIQUEIDENTIFIER NOT NULL");
			sqlStringBuilder.AppendLine("		,'+QUOTENAME(@primaryKeyName2)+' UNIQUEIDENTIFIER NOT NULL");
			sqlStringBuilder.AppendLine("		,PRIMARY KEY ('+QUOTENAME(@primaryKeyName1)+','+QUOTENAME(@primaryKeyName2)+')");
			sqlStringBuilder.AppendLine("	)'");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("	IF(@debug = 1)");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		PRINT @sql");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		EXEC SP_EXECUTESQL @sql, N'@tablename NVARCHAR(128), @primaryKeyName1 NVARCHAR(128), @primaryKeyName2 NVARCHAR(128)', @tablename = @tablename, @primaryKeyName1 = @primaryKeyName1, @primaryKeyName2 = @primaryKeyName2");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}
	}
}
