using DataLayer.SqlData.Account;
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

		protected Account InsertAccount(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			Account createdContact = new Account
			{
				name = $"nameTest_{Guid.NewGuid()}",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			createdContact.Insert(sqlConnection);

			return createdContact;
		}

		protected ContactChange InsertContactChange(SqlConnection sqlConnection, Guid contactId, Guid externalContactId, Guid changeProviderId, DateTime createdTime)
		{
			ContactChange contactChangeCreated = new ContactChange(sqlConnection, contactId, externalContactId, changeProviderId)
			{
				Firstname = $"name_{Guid.NewGuid()}",
				CreatedOn = createdTime,
				ModifiedOn = createdTime,
			};

			contactChangeCreated.Insert();

			return contactChangeCreated;
		}

		protected AccountChange InsertAccountChange(SqlConnection sqlConnection, Guid accountId, Guid externalAccountId, Guid changeProviderId, DateTime createdTime)
		{
			AccountChange accountChangeCreated = new AccountChange(sqlConnection, accountId, externalAccountId, changeProviderId)
			{
				name = $"name_{Guid.NewGuid()}",
				CreatedOn = createdTime,
				ModifiedOn = createdTime,
			};

			accountChangeCreated.Insert();

			return accountChangeCreated;
		}
	}
}
