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
				firstname = $"FirstnameTest_{Guid.NewGuid()}",
				lastname = $"LastNameTest_{Guid.NewGuid()}",
				modifiedon = creationDate,
				createdon = creationDate,
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
				modifiedon = creationDate,
				createdon = creationDate,
			};

			createdContact.Insert(sqlConnection);

			return createdContact;
		}

		protected ContactChange InsertContactChange(SqlConnection sqlConnection, Guid contactId, Guid externalContactId, Guid changeProviderId, DateTime createdTime)
		{
			ContactChange contactChangeCreated = new ContactChange(sqlConnection, contactId, externalContactId, changeProviderId)
			{
				firstname = $"name_{Guid.NewGuid()}",
				createdon = createdTime,
				modifiedon = createdTime,
			};

			contactChangeCreated.Insert();

			return contactChangeCreated;
		}

		protected AccountChange InsertAccountChange(SqlConnection sqlConnection, Guid accountId, Guid externalAccountId, Guid changeProviderId, DateTime createdTime)
		{
			AccountChange accountChangeCreated = new AccountChange(sqlConnection, accountId, externalAccountId, changeProviderId)
			{
				name = $"name_{Guid.NewGuid()}",
				createdon = createdTime,
				modifiedon = createdTime,
			};

			accountChangeCreated.Insert();

			return accountChangeCreated;
		}
	}
}
