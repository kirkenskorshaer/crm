using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer.SqlData;
using NUnit.Framework;
using System.Linq;
using DataLayer;
using DataLayer.SqlData.Account;
using System.Data.SqlTypes;
using DataLayer.MongoData;

namespace DataLayerTest.SqlDataTest.AccountTest
{
	[TestFixture]
	public class AccountTest
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

		[Test]
		public void DeleteTest()
		{
			Account createdAccount = AccountInsert(_sqlConnection);

			createdAccount.Delete(_sqlConnection);

			List<Account> Accounts = Account.ReadLatest(_sqlConnection, createdAccount.CreatedOn.AddSeconds(-1));
			Assert.AreEqual(0, Accounts.Count);
		}

		[Test]
		public void ReadLatestTest()
		{
			Account createdAccount = AccountInsert(_sqlConnection);

			List<Account> Accounts = Account.ReadLatest(_sqlConnection, createdAccount.CreatedOn.AddSeconds(-1));

			Assert.AreEqual(1, Accounts.Count);
		}

		[Test]
		public void ReadReadsCorrectAccount()
		{
			Account createdAccount1 = AccountInsert(_sqlConnection);
			Account createdAccount2 = AccountInsert(_sqlConnection);
			Account createdAccount3 = AccountInsert(_sqlConnection);
			Account createdAccount4 = AccountInsert(_sqlConnection);
			Account createdAccount5 = AccountInsert(_sqlConnection);

			Account Account3Read = Account.Read(_sqlConnection, createdAccount3.Id);

			createdAccount1.Delete(_sqlConnection);
			createdAccount2.Delete(_sqlConnection);
			createdAccount3.Delete(_sqlConnection);
			createdAccount4.Delete(_sqlConnection);
			createdAccount5.Delete(_sqlConnection);

			Assert.AreEqual(createdAccount3.name, Account3Read.name);
		}

		[Test]
		public void AccountCanBeRestoredWithoutOptionalColumn()
		{
			Account createdAccount = AccountInsertWithoutLastname(_sqlConnection);

			Account AccountRead = Account.Read(_sqlConnection, createdAccount.Id);

			createdAccount.Delete(_sqlConnection);

			Assert.AreEqual(createdAccount.name, AccountRead.name);
		}

		[Test]
		public void AccountCanBeUpdated()
		{
			Account createdAccount = AccountInsert(_sqlConnection);
			createdAccount.name = "newName";

			createdAccount.Update(_sqlConnection);

			Account AccountRead = Account.Read(_sqlConnection, createdAccount.Id);

			createdAccount.Delete(_sqlConnection);

			Assert.AreEqual(createdAccount.name, AccountRead.name);
		}

		[Test]
		public void ReadNextByIdReturnsNullWhenThereIsNoAccounts()
		{
			Account readAccount = Account.ReadNextById(_sqlConnection, Guid.Empty);

			Assert.IsNull(readAccount);
		}

		[Test]
		public void ReadNextByIdReturnsAccountsInOrder()
		{
			Account createdAccount1 = AccountInsert(_sqlConnection);
			Account createdAccount2 = AccountInsert(_sqlConnection);
			Account createdAccount3 = AccountInsert(_sqlConnection);
			Account createdAccount4 = AccountInsert(_sqlConnection);

			Account readAccount1 = Account.ReadNextById(_sqlConnection, Guid.Empty);
			Account readAccount2 = Account.ReadNextById(_sqlConnection, readAccount1.Id);
			Account readAccount3 = Account.ReadNextById(_sqlConnection, readAccount2.Id);
			Account readAccount4 = Account.ReadNextById(_sqlConnection, readAccount3.Id);
			Account readAccount5 = Account.ReadNextById(_sqlConnection, readAccount4.Id);

			SqlGuid[] sortedGuids = new List<Account>() { createdAccount1, createdAccount2, createdAccount3, createdAccount4 }.Select(Account => new SqlGuid(Account.Id)).OrderBy(sqlGuid => sqlGuid).ToArray();

			Assert.AreEqual(sortedGuids[0], new SqlGuid(readAccount1.Id));
			Assert.AreEqual(sortedGuids[1], new SqlGuid(readAccount2.Id));
			Assert.AreEqual(sortedGuids[2], new SqlGuid(readAccount3.Id));
			Assert.AreEqual(sortedGuids[3], new SqlGuid(readAccount4.Id));
			Assert.AreEqual(sortedGuids[0], new SqlGuid(readAccount5.Id));
		}

		[Test]
		public void InsertCreatesProgress()
		{
			Account createdAccount1 = AccountInsert(_sqlConnection, true);

			bool progressExists = Progress.Exists(_mongoConnection, "Account", createdAccount1.Id);

			Assert.IsTrue(progressExists);
		}

		private Account AccountInsertWithoutLastname(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			Account createdAccount = new Account
			{
				name = $"name_{Guid.NewGuid()}",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			createdAccount.Insert(sqlConnection);
			return createdAccount;
		}

		internal Account AccountInsert(SqlConnection sqlConnection, bool useMongoConnection = false)
		{
			DateTime creationDate = DateTime.Now;

			Account createdAccount = new Account
			{
				name = $"name_{Guid.NewGuid()}",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			if (useMongoConnection)
			{
				createdAccount.Insert(sqlConnection, _mongoConnection);
			}
			else
			{
				createdAccount.Insert(sqlConnection);
			}
			return createdAccount;
		}
	}
}
