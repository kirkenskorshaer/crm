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
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeGroup();

			List<Group> groups = Group.ReadGroupsFromAccountChange(_sqlConnection, arrangedData.AccountChange.Id);
			arrangedData.AccountChangeGroup.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.Group.Id, groups.Single().Id);
		}

		[Test]
		public void Delete()
		{
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeGroup();

			arrangedData.AccountChangeGroup.Delete(_sqlConnection);
			List<Group> groups = Group.ReadGroupsFromAccountChange(_sqlConnection, arrangedData.AccountChange.Id);

			Assert.IsFalse(groups.Any());
		}

		private DatabaseArrangeResponse ArrangeAccountChangeGroup()
		{
			DatabaseArrangeResponse response = new DatabaseArrangeResponse();

			response.Account = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			response.Account.Insert(_sqlConnection);

			response.ChangeProvider = new ChangeProvider()
			{
				Name = "test",
			};
			response.ChangeProvider.Insert(_sqlConnection);

			response.ExternalAccount = new ExternalAccount(_sqlConnection, Guid.NewGuid(), response.ChangeProvider.Id);
			response.ExternalAccount.Insert();

			AccountChangeTest accountChangeTest = new AccountChangeTest();
			accountChangeTest.TestFixtureSetUp();
			response.AccountChange = accountChangeTest.AccountChangeInsert(response.ExternalAccount, response.Account, DateTime.Now);
			response.AccountChange.Insert();

			response.Group = new GroupTest().GroupInsert(_sqlConnection);

			response.AccountChangeGroup = new AccountChangeGroup(response.AccountChange.Id, response.Group.Id);
			response.AccountChangeGroup.Insert(_sqlConnection);

			return response;
        }
	}
}
