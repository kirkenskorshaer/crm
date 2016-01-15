using DataLayer;
using DataLayer.SqlData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer.SqlData.Account;
using System.Linq;

namespace DataLayerTest.SqlDataTest.AccountTest
{
	[TestFixture]
	public class AccountChangeTest : TestSqlBase
	{
		private MongoConnection _mongoConnection;
		private SqlConnection _sqlConnection;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");

			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
		}

		[SetUp]
		public void SetUp()
		{
			Utilities.RecreateAllTables(_sqlConnection);
		}

		internal ExternalAccount InsertExternalAccount(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			ChangeProvider changeProvider = new ChangeProvider();
			changeProvider.Name = $"name_{Guid.NewGuid()}";

			changeProvider.Insert(sqlConnection);

			Guid changeProviderId = changeProvider.Id;

			ExternalAccount createdExternalAccount = new ExternalAccount(sqlConnection, Guid.NewGuid(), changeProviderId);

			createdExternalAccount.Insert();

			return createdExternalAccount;
		}

		[TestCase(AccountChange.IdType.ChangeProviderId)]
		[TestCase(AccountChange.IdType.AccountChangeId)]
		[TestCase(AccountChange.IdType.AccountId)]
		[TestCase(AccountChange.IdType.ExternalAccountId)]
		public void ReadReadsInserted(AccountChange.IdType idType)
		{
			ExternalAccount externalAccountCreated = InsertExternalAccount(_sqlConnection);
			Account AccountCreated = InsertAccount(_sqlConnection);
			DateTime createdTime = DateTime.Now;

			AccountChange AccountChangeCreated = AccountChangeInsert(externalAccountCreated, AccountCreated, createdTime);

			AccountChangeCreated.Insert();

			Guid id = Guid.Empty;
			switch (idType)
			{
				case AccountChange.IdType.AccountChangeId:
					id = AccountChangeCreated.Id;
					break;
				case AccountChange.IdType.AccountId:
					id = AccountCreated.Id;
					break;
				case AccountChange.IdType.ExternalAccountId:
					id = externalAccountCreated.ExternalAccountId;
					break;
				case AccountChange.IdType.ChangeProviderId:
					id = externalAccountCreated.ChangeProviderId;
					break;
				default:
					break;
			}

			List<AccountChange> AccountChangesRead = AccountChange.Read(_sqlConnection, id, idType);

			AccountChangeCreated.Delete(_sqlConnection);

			Assert.AreEqual(AccountChangeCreated.name, AccountChangesRead.Single().name);
		}

		internal AccountChange AccountChangeInsert(ExternalAccount externalAccountCreated, Account AccountCreated, DateTime createdTime)
		{
			return new AccountChange(_sqlConnection, AccountCreated.Id, externalAccountCreated.ExternalAccountId, externalAccountCreated.ChangeProviderId)
			{
				name = $"name_{Guid.NewGuid()}",
				CreatedOn = createdTime,
				ModifiedOn = createdTime,
			};
		}

		[Test]
		public void GetAccountsReturnsAccounts()
		{
			ExternalAccount externalAccount1Created = InsertExternalAccount(_sqlConnection);
			ExternalAccount externalAccount2Created = InsertExternalAccount(_sqlConnection);

			Account Account1Created = InsertAccount(_sqlConnection);
			Account Account2Created = InsertAccount(_sqlConnection);

			DateTime creationDate = DateTime.Now;

			AccountChange AccountChange1_1Created = new AccountChange(_sqlConnection, Account1Created.Id, externalAccount1Created.ExternalAccountId, externalAccount1Created.ChangeProviderId)
			{
				name = "test",
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			AccountChange1_1Created.Insert();

			AccountChange AccountChange1_2Created = new AccountChange(_sqlConnection, Account1Created.Id, externalAccount2Created.ExternalAccountId, externalAccount2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			AccountChange1_2Created.Insert();

			AccountChange AccountChange2Created = new AccountChange(_sqlConnection, Account2Created.Id, externalAccount2Created.ExternalAccountId, externalAccount2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			AccountChange2Created.Insert();

			List<Account> AccountsChangedByExternalAccount2 = AccountChange.GetAccounts(_sqlConnection, externalAccount2Created.ExternalAccountId);

			AccountChange1_1Created.Delete(_sqlConnection);
			AccountChange1_2Created.Delete(_sqlConnection);
			AccountChange2Created.Delete(_sqlConnection);

			Assert.True(AccountsChangedByExternalAccount2.Any(Account => Account.Id == Account1Created.Id));
			Assert.True(AccountsChangedByExternalAccount2.Any(Account => Account.Id == Account2Created.Id));
		}

		[Test]
		public void GetExternalAccounts()
		{
			ExternalAccount externalAccount1Created = InsertExternalAccount(_sqlConnection);
			ExternalAccount externalAccount2Created = InsertExternalAccount(_sqlConnection);

			Account Account1Created = InsertAccount(_sqlConnection);
			Account Account2Created = InsertAccount(_sqlConnection);

			DateTime creationDate = DateTime.Now;

			AccountChange AccountChange1_1Created = new AccountChange(_sqlConnection, Account1Created.Id, externalAccount1Created.ExternalAccountId, externalAccount1Created.ChangeProviderId)
			{
				name = "test",
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			AccountChange1_1Created.Insert();

			AccountChange AccountChange1_2Created = new AccountChange(_sqlConnection, Account1Created.Id, externalAccount2Created.ExternalAccountId, externalAccount2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			AccountChange1_2Created.Insert();

			AccountChange AccountChange2Created = new AccountChange(_sqlConnection, Account2Created.Id, externalAccount2Created.ExternalAccountId, externalAccount2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			AccountChange2Created.Insert();

			List<ExternalAccount> externalAccountsChangingAccount1 = AccountChange.GetExternalAccounts(_sqlConnection, Account1Created.Id);

			AccountChange1_1Created.Delete(_sqlConnection);
			AccountChange1_2Created.Delete(_sqlConnection);
			AccountChange2Created.Delete(_sqlConnection);

			Assert.True(externalAccountsChangingAccount1.Any(externalAccount => externalAccount.ExternalAccountId == externalAccount1Created.ExternalAccountId && externalAccount.ChangeProviderId == externalAccount1Created.ChangeProviderId));
			Assert.True(externalAccountsChangingAccount1.Any(externalAccount => externalAccount.ExternalAccountId == externalAccount2Created.ExternalAccountId && externalAccount.ChangeProviderId == externalAccount2Created.ChangeProviderId));
		}
	}
}
