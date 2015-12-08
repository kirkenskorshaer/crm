using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Contact;
using DataLayer.SqlData.Group;
using DataLayerTest.SqlDataTest.ContactTest;
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
	public class ContactChangeGroupTest
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

			ChangeProvider changeProvider = new ChangeProvider()
			{
				Name = "test",
			};
			changeProvider.Insert(_sqlConnection);

			ExternalContact externalContactCreated = new ExternalContact(_sqlConnection, Guid.NewGuid(), changeProvider.Id);
			externalContactCreated.Insert();

			ContactChangeTest contactChangeTest = new ContactChangeTest();
			contactChangeTest.TestFixtureSetUp();
			ContactChange contactChangeInserted = contactChangeTest.ContactChangeInsert(externalContactCreated, contactInserted, DateTime.Now);
			contactChangeInserted.Insert();

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			ContactChangeGroup contactChangeGroup = new ContactChangeGroup(contactChangeInserted.Id, groupInserted.Id);
			contactChangeGroup.Insert(_sqlConnection);

			List<Group> groups = Group.ReadGroupsFromContactChange(_sqlConnection, contactChangeInserted.Id);
			contactChangeGroup.Delete(_sqlConnection);

			Assert.AreEqual(groupInserted.Id, groups.Single().Id);
		}

		[Test]
		public void Delete()
		{
			Contact contactInserted = new ContactTest.ContactTest().ContactInsert(_sqlConnection);
			contactInserted.Insert(_sqlConnection);

			ChangeProvider changeProvider = new ChangeProvider()
			{
				Name = "test",
			};
			changeProvider.Insert(_sqlConnection);

			ExternalContact externalContactCreated = new ExternalContact(_sqlConnection, Guid.NewGuid(), changeProvider.Id);
			externalContactCreated.Insert();

			ContactChangeTest contactChangeTest = new ContactChangeTest();
			contactChangeTest.TestFixtureSetUp();
            ContactChange contactChangeInserted = contactChangeTest.ContactChangeInsert(externalContactCreated, contactInserted, DateTime.Now);
            contactChangeInserted.Insert();

			Group groupInserted = new GroupTest().GroupInsert(_sqlConnection);

			ContactChangeGroup contactChangeGroup = new ContactChangeGroup(contactChangeInserted.Id, groupInserted.Id);
			contactChangeGroup.Insert(_sqlConnection);

			contactChangeGroup.Delete(_sqlConnection);
			List<Group> groups = Group.ReadGroupsFromContactChange(_sqlConnection, contactChangeInserted.Id);

			Assert.IsFalse(groups.Any());
		}
	}
}
