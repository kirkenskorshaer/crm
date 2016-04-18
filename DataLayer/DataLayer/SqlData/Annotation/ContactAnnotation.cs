using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.SqlData.Annotation
{
	public class ContactAnnotation : AbstractIdData
	{
		[SqlColumn(SqlColumn.PropertyEnum.None, SqlUtilities.DataType.NVARCHAR_MAX, false)]
		public string notetext;

		[SqlColumn(SqlColumn.PropertyEnum.ForeignKey, SqlUtilities.DataType.UNIQUEIDENTIFIER, false, "contactid", typeof(Contact.Contact), "id", true, 1)]
		public Guid ContactId;

		public ContactAnnotation()
		{
		}

		public ContactAnnotation(Guid contactId)
		{
			ContactId = contactId;
		}

		public static void MaintainTable(SqlConnection sqlConnection)
		{
			Type dataClassType = typeof(ContactAnnotation);

			SqlUtilities.MaintainTable(sqlConnection, dataClassType);
		}

		public static ContactAnnotation Read(SqlConnection sqlConnection, Guid id)
		{
			List<ContactAnnotation> annotations = Read<ContactAnnotation>(sqlConnection, "id", id);

			return annotations.Single();
		}

		public static bool ExistsById(SqlConnection sqlConnection, Guid id)
		{
			return Exists<ContactAnnotation>(sqlConnection, "id", id);
		}

		public static List<ContactAnnotation> ReadByContactId(SqlConnection sqlConnection, Guid contactId)
		{
			return Read<ContactAnnotation>(sqlConnection, new List<SqlCondition>()
			{
				new SqlCondition("ContactId", "=", contactId),
			});
		}
	}
}
