using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Contact;
using DataLayer.SqlData.Group;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.GroupTest
{
	[TestFixture]
	public class GroupTest
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
		public void ExistsByNameReturnsTrueForExistingGroup()
		{
			Group createdGroup = GroupInsert(_sqlConnection);

			bool groupExists = Group.ExistsByName(_sqlConnection, createdGroup.Name);

			createdGroup.Delete(_sqlConnection);
			Assert.IsTrue(groupExists);
		}

		[Test]
		public void ExistsByNameReturnsFalseForDeletedGroup()
		{
			Group createdGroup = GroupInsert(_sqlConnection);
			createdGroup.Delete(_sqlConnection);

			bool groupExists = Group.ExistsByName(_sqlConnection, createdGroup.Name);

			Assert.IsFalse(groupExists);
		}

		[Test]
		public void ReadGroupsFromContact()
		{
			ContactTest.ContactTest contactTest = new ContactTest.ContactTest();
			Contact contactInserted = contactTest.ContactInsert(_sqlConnection);

			Group groupInserted1 = GroupInsert(_sqlConnection);
			Group groupInserted2 = GroupInsert(_sqlConnection);
			Group groupInserted3 = GroupInsert(_sqlConnection);

			ContactGroup contactGroup1 = new ContactGroup(contactInserted.Id, groupInserted1.Id);
			contactGroup1.Insert(_sqlConnection);

			ContactGroup contactGroup2 = new ContactGroup(contactInserted.Id, groupInserted2.Id);
			contactGroup2.Insert(_sqlConnection);

			List<Group> groups = Group.ReadGroupsFromContact(_sqlConnection, contactInserted.Id);

			Assert.IsTrue(groups.Any(group => group.Name == groupInserted1.Name));
			Assert.IsTrue(groups.Any(group => group.Name == groupInserted2.Name));
			Assert.IsFalse(groups.Any(group => group.Name == groupInserted3.Name));
		}

		[Test]
		public void ReadGroupsFromContactChange()
		{
			ContactTest.ContactChangeTest contactChangeTest = new ContactTest.ContactChangeTest();
			contactChangeTest.TestFixtureSetUp();
			ContactTest.ContactTest contactTest = new ContactTest.ContactTest();

			Contact contactInserted = contactTest.ContactInsert(_sqlConnection);
			ExternalContact externalContact = contactChangeTest.InsertExternalContact(_sqlConnection, contactInserted.Id);
			ContactChange contactChangeInserted = contactChangeTest.ContactChangeInsert(externalContact, contactInserted, DateTime.Now);
			contactChangeInserted.Insert();

			Group groupInserted1 = GroupInsert(_sqlConnection);
			Group groupInserted2 = GroupInsert(_sqlConnection);
			Group groupInserted3 = GroupInsert(_sqlConnection);

			ContactChangeGroup contactChangeGroup1 = new ContactChangeGroup(contactChangeInserted.Id, groupInserted1.Id);
			contactChangeGroup1.Insert(_sqlConnection);

			ContactChangeGroup contactChangeGroup2 = new ContactChangeGroup(contactChangeInserted.Id, groupInserted2.Id);
			contactChangeGroup2.Insert(_sqlConnection);

			List<Group> groups = Group.ReadGroupsFromContactChange(_sqlConnection, contactChangeInserted.Id);

			Assert.IsTrue(groups.Any(group => group.Name == groupInserted1.Name));
			Assert.IsTrue(groups.Any(group => group.Name == groupInserted2.Name));
			Assert.IsFalse(groups.Any(group => group.Name == groupInserted3.Name));
		}

		internal Group GroupInsert(SqlConnection _sqlConnection)
		{
			Group createdGroup = new Group()
			{
				Name = $"name_{Guid.NewGuid()}",
			};

			createdGroup.Insert(_sqlConnection);

			return createdGroup;
		}
	}
}
