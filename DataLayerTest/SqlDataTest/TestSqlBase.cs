using DataLayer.SqlData.Contact;
using System;
using System.Data.SqlClient;

namespace DataLayerTest.SqlDataTest
{
	public abstract class TestSqlBase
	{
		protected Contact InsertContact(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			Contact createdContact = new Contact
			{
				Firstname = $"FirstnameTest_{Guid.NewGuid()}",
				Lastname = $"LastNameTest_{Guid.NewGuid()}",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			createdContact.Insert(sqlConnection);

			return createdContact;
		}
	}
}
