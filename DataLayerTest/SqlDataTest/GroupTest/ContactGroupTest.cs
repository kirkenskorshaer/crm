using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Contact;
using DataLayer.SqlData.Group;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.GroupTest
{
	[TestFixture]
	public class ContactGroupTest
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
			Contact contactInserted = new ContactTest.ContactTest().ContactInsert(_sqlConnection);
			contactInserted.Insert(_sqlConnection);

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			ContactGroup contactGroup = new ContactGroup(contactInserted.Id, groupInserted.Id);
			contactGroup.Insert(_sqlConnection);

			List<Group> groups = Group.ReadGroupsFromContact(_sqlConnection, contactInserted.Id);
			contactGroup.Delete(_sqlConnection);

			Assert.AreEqual(groupInserted.Id, groups.Single().Id);
		}

		[Test]
		public void Delete()
		{
			Contact contactInserted = new ContactTest.ContactTest().ContactInsert(_sqlConnection);
			contactInserted.Insert(_sqlConnection);

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			ContactGroup contactGroup = new ContactGroup(contactInserted.Id, groupInserted.Id);
			contactGroup.Insert(_sqlConnection);

			contactGroup.Delete(_sqlConnection);
			List<Group> groups = Group.ReadGroupsFromContact(_sqlConnection, contactInserted.Id);

			Assert.IsFalse(groups.Any());
		}
	}
}
