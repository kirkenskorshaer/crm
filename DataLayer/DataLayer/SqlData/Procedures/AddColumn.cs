using System.Data.SqlClient;
using System.Text;

namespace DataLayer.SqlData.Procedures
{
	internal static class AddColumn
	{
		internal static void MakeSureAddColumnProcedureArePresent(SqlConnection sqlConnection)
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
			sqlStringBuilder.AppendLine("			objects.name = 'AddColumn'");
			sqlStringBuilder.AppendLine("	)");
			sqlStringBuilder.AppendLine(")");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DROP PROCEDURE AddColumn");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}

		private static void CreateProcedure(SqlConnection sqlConnection)
		{
			StringBuilder sqlStringBuilder = new StringBuilder();
			sqlStringBuilder = new StringBuilder();

			sqlStringBuilder.AppendLine("CREATE PROCEDURE");
			sqlStringBuilder.AppendLine("	AddColumn");
			sqlStringBuilder.AppendLine("	@tablename NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@columnName NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@type NVARCHAR(128)");
			sqlStringBuilder.AppendLine("	,@allowNull BIT = 0");
			sqlStringBuilder.AppendLine("	,@debug BIT = 1");
			sqlStringBuilder.AppendLine("AS");
			sqlStringBuilder.AppendLine("BEGIN");
			sqlStringBuilder.AppendLine("	DECLARE @sql nvarchar(4000)");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("	SET @sql = '");
			sqlStringBuilder.AppendLine("	ALTER TABLE");
			sqlStringBuilder.AppendLine("		dbo.'+ QUOTENAME(@tablename) + '");
			sqlStringBuilder.AppendLine("	ADD");
			sqlStringBuilder.AppendLine("		'+QUOTENAME(@columnName)");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("	IF (QUOTENAME(@type) = '[NVARCHAR(MAX)]')");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		SET @sql = @sql + ' NVARCHAR(MAX)'");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE IF (QUOTENAME(@type) = '[INT]')");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		SET @sql = @sql + ' INT'");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE IF (QUOTENAME(@type) = '[DATETIME]')");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		SET @sql = @sql + ' DATETIME'");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE IF (QUOTENAME(@type) = '[UNIQUEIDENTIFIER]')");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		SET @sql = @sql + ' UNIQUEIDENTIFIER'");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		DECLARE @error NVARCHAR(MAX) = 'unknown type ' + QUOTENAME(@type)");
			sqlStringBuilder.AppendLine("		RAISERROR (@error,11,1)");
			sqlStringBuilder.AppendLine("		RETURN");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("	IF(@allowNull = 1)");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		SET @sql = @sql + ' NULL'");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		SET @sql = @sql + ' NOT NULL'");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("");
			sqlStringBuilder.AppendLine("	IF(@debug = 1)");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		PRINT @sql");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("	ELSE");
			sqlStringBuilder.AppendLine("	BEGIN");
			sqlStringBuilder.AppendLine("		EXEC SP_EXECUTESQL @sql, N'@tablename NVARCHAR(128), @columnName NVARCHAR(128), @type NVARCHAR(128)', @tablename = @tablename, @columnName = @columnName, @type = @type");
			sqlStringBuilder.AppendLine("	END");
			sqlStringBuilder.AppendLine("END");

			Utilities.ExecuteNonQuery(sqlConnection, sqlStringBuilder, System.Data.CommandType.Text);
		}
	}
}
