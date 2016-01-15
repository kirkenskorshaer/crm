using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Account;
using DataLayer.SqlData.Group;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.GroupTest
{
	[TestFixture]
	public class AccountGroupTest
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

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			AccountGroup accountGroup = new AccountGroup(accountInserted.Id, groupInserted.Id);
			accountGroup.Insert(_sqlConnection);

			List<Group> groups = Group.ReadGroupsFromAccount(_sqlConnection, accountInserted.Id);
			accountGroup.Delete(_sqlConnection);

			Assert.AreEqual(groupInserted.Id, groups.Single().Id);
		}

		[Test]
		public void Delete()
		{
			Account accountInserted = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			accountInserted.Insert(_sqlConnection);

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			AccountGroup accountGroup = new AccountGroup(accountInserted.Id, groupInserted.Id);
			accountGroup.Insert(_sqlConnection);

			accountGroup.Delete(_sqlConnection);
			List<Group> groups = Group.ReadGroupsFromAccount(_sqlConnection, accountInserted.Id);

			Assert.IsFalse(groups.Any());
		}
	}
}
