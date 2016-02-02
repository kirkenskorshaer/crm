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
			DatabaseArrangeResponse arrangedData = ArrangeContactChangeGroup();

			List<Group> groups = Group.ReadGroupsFromContactChange(_sqlConnection, arrangedData.ContactChange.Id);
			arrangedData.ContactChangeGroup.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.Group.Id, groups.Single().Id);
		}

		[Test]
		public void Delete()
		{
			DatabaseArrangeResponse arrangedData = ArrangeContactChangeGroup();

			arrangedData.ContactChangeGroup.Delete(_sqlConnection);
			List<Group> groups = Group.ReadGroupsFromContactChange(_sqlConnection, arrangedData.ContactChange.Id);

			Assert.IsFalse(groups.Any());
		}

		[Test]
		public void ReadFromGroupId()
		{
			DatabaseArrangeResponse arrangedData = ArrangeContactChangeGroup();

			List<ContactChangeGroup> contactChangeGroups = ContactChangeGroup.ReadFromGroupId(_sqlConnection, arrangedData.Group.Id);
			arrangedData.ContactChangeGroup.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.ContactChangeGroup, contactChangeGroups.Single());
		}

		[Test]
		public void ReadFromContactChangeId()
		{
			DatabaseArrangeResponse arrangedData = ArrangeContactChangeGroup();

			List<ContactChangeGroup> contactChangeGroups = ContactChangeGroup.ReadFromContactChangeId(_sqlConnection, arrangedData.ContactChange.Id);
			arrangedData.ContactChangeGroup.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.ContactChangeGroup, contactChangeGroups.Single());
		}

		private DatabaseArrangeResponse ArrangeContactChangeGroup()
		{
			DatabaseArrangeResponse response = new DatabaseArrangeResponse();

			response.Contact = new ContactTest.ContactTest().ContactInsert(_sqlConnection);
			response.Contact.Insert(_sqlConnection);

			response.ChangeProvider = new ChangeProvider()
			{
				Name = "test",
			};
			response.ChangeProvider.Insert(_sqlConnection);

			response.ExternalContact = new ExternalContact(_sqlConnection, Guid.NewGuid(), response.ChangeProvider.Id, response.Contact.Id);
			response.ExternalContact.Insert();

			ContactChangeTest contactChangeTest = new ContactChangeTest();
			contactChangeTest.TestFixtureSetUp();
			response.ContactChange = contactChangeTest.ContactChangeInsert(response.ExternalContact, response.Contact, DateTime.Now);
			response.ContactChange.Insert();

			response.Group = new GroupTest().GroupInsert(_sqlConnection);

			response.ContactChangeGroup = new ContactChangeGroup(response.ContactChange.Id, response.Group.Id);
			response.ContactChangeGroup.Insert(_sqlConnection);

			return response;
		}
	}
}
