using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Account;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.AccountTest
{
	[TestFixture]
	public class ExternalAccountTest : TestSqlBase
	{
		private MongoConnection _mongoConnection;
		private SqlConnection _sqlConnection;

		private List<ChangeProvider> _changeProviders = new List<ChangeProvider>();

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");

			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
		}

		[SetUp]
		public void SetUp()
		{
			SqlUtilities.RecreateAllTables(_sqlConnection);
		}

		[TearDown]
		public void TearDown()
		{
			_changeProviders.ForEach(changeProvider => changeProvider.Delete(_sqlConnection));

			_changeProviders.Clear();
		}

		private ChangeProvider InsertChangeProvider()
		{
			ChangeProvider changeProvider = new ChangeProvider();
			changeProvider.Name = $"name_{Guid.NewGuid()}";
			changeProvider.Insert(_sqlConnection);

			_changeProviders.Add(changeProvider);

			return changeProvider;
		}

		[Test]
		public void ReadReadsInserted()
		{
			ChangeProvider changeProvider = InsertChangeProvider();
			Guid externalAccountId = Guid.NewGuid();

			Account account = new AccountTest().AccountInsert(_sqlConnection);

			ExternalAccount externalAccountCreated = new ExternalAccount(_sqlConnection, externalAccountId, changeProvider.Id, account.Id);
			externalAccountCreated.Insert();

			ExternalAccount externalAccountRead = ExternalAccount.Read(_sqlConnection, externalAccountId, changeProvider.Id);

			Assert.AreEqual(externalAccountCreated.ChangeProviderId, externalAccountRead.ChangeProviderId);
			Assert.AreEqual(externalAccountCreated.ExternalAccountId, externalAccountRead.ExternalAccountId);
		}

		[Test]
		public void ReadReadsAListFromChangeProviderId()
		{
			ChangeProvider changeProvider = InsertChangeProvider();

			Account account = new AccountTest().AccountInsert(_sqlConnection);

			Guid externalAccount1Id = Guid.NewGuid();
			ExternalAccount externalAccount1Created = new ExternalAccount(_sqlConnection, externalAccount1Id, changeProvider.Id, account.Id);
			externalAccount1Created.Insert();

			Guid externalAccount2Id = Guid.NewGuid();
			ExternalAccount externalAccount2Created = new ExternalAccount(_sqlConnection, externalAccount2Id, changeProvider.Id, account.Id);
			externalAccount2Created.Insert();

			List<ExternalAccount> externalAccountReadList = ExternalAccount.Read(_sqlConnection, changeProvider.Id);

			Assert.AreEqual(2, externalAccountReadList.Count);
			Assert.IsTrue(externalAccountReadList.Any(externalAccount => externalAccount.ExternalAccountId == externalAccount1Id));
			Assert.IsTrue(externalAccountReadList.Any(externalAccount => externalAccount.ExternalAccountId == externalAccount2Id));
		}

		[Test]
		public void ReadFromChangeProviderAndAccount()
		{
			ChangeProvider changeProvider = InsertChangeProvider();

			Account account = new AccountTest().AccountInsert(_sqlConnection);

			Guid externalAccountId = Guid.NewGuid();
			ExternalAccount externalAccountCreated = new ExternalAccount(_sqlConnection, externalAccountId, changeProvider.Id, account.Id);
			externalAccountCreated.Insert();

			InsertAccountChange(_sqlConnection, account.Id, externalAccountId, changeProvider.Id, DateTime.Now);
			InsertAccountChange(_sqlConnection, account.Id, externalAccountId, changeProvider.Id, DateTime.Now);

			List<ExternalAccount> externalAccountRead = ExternalAccount.ReadFromChangeProviderAndAccount(_sqlConnection, changeProvider.Id, account.Id);

			Assert.AreEqual(externalAccountCreated.ChangeProviderId, externalAccountRead.Single().ChangeProviderId);
			Assert.AreEqual(externalAccountCreated.ExternalAccountId, externalAccountRead.Single().ExternalAccountId);
		}
	}
}
