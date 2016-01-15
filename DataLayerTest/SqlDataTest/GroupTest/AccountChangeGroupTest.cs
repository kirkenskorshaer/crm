using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Account;
using DataLayer.SqlData.Group;
using DataLayerTest.SqlDataTest.AccountTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest.SqlDataTest.GroupTest
{
	[TestFixture]
	public class AccountChangeGroupTest
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
		public void Insert()
		{
			Account accountInserted = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			accountInserted.Insert(_sqlConnection);

			ChangeProvider changeProvider = new ChangeProvider()
			{
				Name = "test",
			};
			changeProvider.Insert(_sqlConnection);

			ExternalAccount externalAccountCreated = new ExternalAccount(_sqlConnection, Guid.NewGuid(), changeProvider.Id);
			externalAccountCreated.Insert();

			AccountChangeTest accountChangeTest = new AccountChangeTest();
			accountChangeTest.TestFixtureSetUp();
			AccountChange accountChangeInserted = accountChangeTest.AccountChangeInsert(externalAccountCreated, accountInserted, DateTime.Now);
			accountChangeInserted.Insert();

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			AccountChangeGroup accountChangeGroup = new AccountChangeGroup(accountChangeInserted.Id, groupInserted.Id);
			accountChangeGroup.Insert(_sqlConnection);

			List<Group> groups = Group.ReadGroupsFromAccountChange(_sqlConnection, accountChangeInserted.Id);
			accountChangeGroup.Delete(_sqlConnection);

			Assert.AreEqual(groupInserted.Id, groups.Single().Id);
		}

		[Test]
		public void Delete()
		{
			Account accountInserted = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			accountInserted.Insert(_sqlConnection);

			ChangeProvider changeProvider = new ChangeProvider()
			{
				Name = "test",
			};
			changeProvider.Insert(_sqlConnection);

			ExternalAccount externalAccountCreated = new ExternalAccount(_sqlConnection, Guid.NewGuid(), changeProvider.Id);
			externalAccountCreated.Insert();

			AccountChangeTest accountChangeTest = new AccountChangeTest();
			accountChangeTest.TestFixtureSetUp();
			AccountChange accountChangeInserted = accountChangeTest.AccountChangeInsert(externalAccountCreated, accountInserted, DateTime.Now);
			accountChangeInserted.Insert();

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			AccountChangeGroup accountChangeGroup = new AccountChangeGroup(accountChangeInserted.Id, groupInserted.Id);
			accountChangeGroup.Insert(_sqlConnection);

			accountChangeGroup.Delete(_sqlConnection);
			List<Group> groups = Group.ReadGroupsFromAccountChange(_sqlConnection, accountChangeInserted.Id);

			Assert.IsFalse(groups.Any());
		}
	}
}
