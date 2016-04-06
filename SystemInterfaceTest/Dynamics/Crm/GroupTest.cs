using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class GroupTest
	{
		private DynamicsCrmConnection _connection;

		[SetUp]
		public void SetUp()
		{
			MongoConnection connection = MongoConnection.GetConnection("test");
			UrlLogin login = UrlLogin.GetUrlLogin(connection, "test");
			_connection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
		}

		[Test]
		[Ignore]
		public void GetAllAttributeNamesTest()
		{
			DateTime testDate = DateTime.Now;
			Group groupInserted = CreateTestGroup(testDate);
			groupInserted.Insert(_connection);

			List<string> attributeNames = Group.GetAllAttributeNames(_connection, groupInserted.GroupId);

			groupInserted.Delete(_connection);

			Assert.True(attributeNames.Count > 10);
			Assert.True(attributeNames.Any(name => name == "new_groupid"));

			attributeNames.ForEach(name => Console.Out.WriteLine(name));
		}

		[Test]
		public void InsertCreatesNewGroup()
		{
			DateTime testDate = DateTime.Now;
			Group groupInserted = CreateTestGroup(testDate);

			groupInserted.Insert(_connection);
			List<Group> Groups = Group.Read(_connection, groupInserted.Name);
			groupInserted.Delete(_connection);

			Assert.AreEqual(groupInserted.GroupId, Groups.Single().GroupId);
			Assert.AreNotEqual(Guid.Empty, groupInserted.GroupId);
		}

		[Test]
		public void DeleteRemovesGroup()
		{
			DateTime testDate = DateTime.Now;
			Group groupInserted = CreateTestGroup(testDate);

			groupInserted.Insert(_connection);
			groupInserted.Delete(_connection);
			List<Group> groups = Group.Read(_connection, groupInserted.Name);

			Assert.False(groups.Any(group => group.GroupId == groupInserted.GroupId));
			Assert.AreNotEqual(Guid.Empty, groupInserted.GroupId);
		}

		[Test]
		public void UpdateUpdatesData()
		{
			DateTime testDate = DateTime.Now;
			Group groupInserted = CreateTestGroup(testDate);
			string nameTest = "nameTest";

			groupInserted.Insert(_connection);
			groupInserted.Name = nameTest;

			groupInserted.Update(_connection);

			Group groupRead = Group.Read(_connection, groupInserted.GroupId);
			groupInserted.Delete(_connection);

			Assert.AreEqual(nameTest, groupRead.Name);
		}

		private Group CreateTestGroup(DateTime testDate)
		{
			string dateString = testDate.ToString("yyyy_MM_dd_HH_mm_ss");
			Group groupCreated = new Group
			{
				Name = $"name_{dateString}",
			};
			return groupCreated;
		}
	}
}
