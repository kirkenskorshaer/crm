﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Annotation
{
	public class AccountAnnotation : AbstractIdData, IDeletableModifiedIdData, IComparable
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, false)]
		public string notetext;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "accountid", typeof(Account.Account), "id", true, 1)]
		public Guid AccountId;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.DATETIME, false)]
		public DateTime modifiedon { get; set; }

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.BIT, false)]
		public bool isdeleted { get; set; }

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

		public static List<AccountAnnotation> ReadByAccountIdAndIsdeleted(SqlConnection sqlConnection, Guid accountId, bool isdeleted)
		{
			return Read<AccountAnnotation>(sqlConnection, new List<SqlCondition>()
			{
				new SqlCondition("AccountId", "=", accountId),
				new SqlCondition("isdeleted", "=", isdeleted),
			});
		}

		public int CompareTo(object obj)
		{
			AccountAnnotation compareAnnotation = obj as AccountAnnotation;

			return compareAnnotation.notetext.CompareTo(notetext);
		}
	}
}