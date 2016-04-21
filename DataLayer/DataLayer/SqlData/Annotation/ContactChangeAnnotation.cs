using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Annotation
{
	public class ContactChangeAnnotation : AbstractIdData, IDeletableModifiedIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, false)]
		public string notetext;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "contactchangeid", typeof(Contact.ContactChange), "id", true, 1)]
		public Guid ContactChangeId;

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.DATETIME, false)]
		public DateTime modifiedon { get; set; }

		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.BIT, false)]
		public bool isdeleted { get; set; }

		public ContactChangeAnnotation()
		{
		}

		public ContactChangeAnnotation(Guid contactChangeId)
		{
			ContactChangeId = contactChangeId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(ContactChangeAnnotation);

			SqlUtilities.MaintainTable(sqlConnection, dataClassType);
		}

		public static ContactChangeAnnotation Read(SqlConnection sqlConnection, Guid id)
		{
			List<ContactChangeAnnotation> annotations = Read<ContactChangeAnnotation>(sqlConnection, "id", id);

			return annotations.Single();
		}

		public static bool ExistsById(SqlConnection sqlConnection, Guid id)
		{
			return Exists<ContactChangeAnnotation>(sqlConnection, "id", id);
		}
	}
}
