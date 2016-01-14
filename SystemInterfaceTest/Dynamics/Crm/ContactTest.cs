﻿using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class ContactTest
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
		public void ReadLatestTest()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted1 = CreateTestContact(testDate);
			contactInserted1.Insert();
			Contact contactInserted2 = CreateTestContact(testDate);
			contactInserted2.Insert();

			List<Contact> contacts = Contact.ReadLatest(_connection, testDate);

			contactInserted1.Delete();
			contactInserted2.Delete();

			Assert.AreEqual(2, contacts.Count);
		}

		[Test]
		[Ignore]
		public void GetAllAttributeNamesTest()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			contactInserted.Insert();

			List<string> attributeNames = Contact.GetAllAttributeNames(_connection, contactInserted.Id);

			contactInserted.Delete();

			Assert.True(attributeNames.Count > 10);
			Assert.True(attributeNames.Any(name => name == "Guid contactid"));

			attributeNames.ForEach(name => Console.Out.WriteLine(name));
		}

		[Test]
		public void InsertCreatesNewContact()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);

			contactInserted.Insert();
			List<Contact> contacts = Contact.ReadLatest(_connection, testDate);
			contactInserted.Delete();

			Assert.True(contacts.Any(contact => contact.Id == contactInserted.Id));
			Assert.AreNotEqual(Guid.Empty, contactInserted.Id);
		}

		[Test]
		public void DeleteRemovesContact()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);

			contactInserted.Insert();
			contactInserted.Delete();
			List<Contact> contacts = Contact.ReadLatest(_connection, testDate);

			Assert.False(contacts.Any(contact => contact.Id == contactInserted.Id));
			Assert.AreNotEqual(Guid.Empty, contactInserted.Id);
		}

		[Test]
		public void UpdateUpdatesData()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			string firstnameTest = "firstnameTest";

			contactInserted.Insert();
			contactInserted.firstname = firstnameTest;

			contactInserted.Update();

			Contact contactRead = Contact.Read(_connection, contactInserted.Id);
			contactInserted.Delete();

			Assert.AreEqual(firstnameTest, contactRead.firstname);
		}

		[Test]
		public void ContactIsCreatedAsActive()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			contactInserted.Insert();

			Contact contactRead = Contact.Read(_connection, contactInserted.Id);

			contactInserted.Delete();

			Assert.AreEqual(Contact.StateEnum.Active, contactRead.State);
		}

		[Test]
		public void SetActiveSetsState()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			contactInserted.Insert();

			contactInserted.SetActive(false);

			Contact contactRead = Contact.Read(_connection, contactInserted.Id);

			contactInserted.Delete();
			Assert.AreEqual(Contact.StateEnum.Inactive, contactRead.State);
		}

		[Test]
		public void GroupsCanBeAdded()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);

			Group group1 = Group.ReadOrCreate(_connection, "test1");
			Group group2 = Group.ReadOrCreate(_connection, "test2");
			Group group3 = Group.ReadOrCreate(_connection, "test3");

			contactInserted.Groups.Add(group1);
			contactInserted.Groups.Add(group2);

			contactInserted.Insert();

			Contact contactRead = Contact.Read(_connection, contactInserted.Id);

			contactInserted.Delete();
			Assert.IsTrue(contactRead.Groups.Any(group => group.Name == group1.Name));
			Assert.IsTrue(contactRead.Groups.Any(group => group.Name == group2.Name));
			Assert.IsFalse(contactRead.Groups.Any(group => group.Name == group3.Name));
		}

		[Test]
		public void GroupsCanBeDeleted()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);

			Group group1 = Group.ReadOrCreate(_connection, "test1");
			Group group2 = Group.ReadOrCreate(_connection, "test2");
			Group group3 = Group.ReadOrCreate(_connection, "test3");

			contactInserted.Groups.Add(group1);
			contactInserted.Groups.Add(group2);

			contactInserted.Insert();

			contactInserted.Groups.Remove(group2);
			contactInserted.Groups.Add(group3);

			contactInserted.Update();

			Contact contactRead = Contact.Read(_connection, contactInserted.Id);

			contactInserted.Delete();
			Assert.IsTrue(contactRead.Groups.Any(group => group.Name == group1.Name));
			Assert.IsFalse(contactRead.Groups.Any(group => group.Name == group2.Name));
			Assert.IsTrue(contactRead.Groups.Any(group => group.Name == group3.Name));
		}

		internal Contact CreateTestContact(DateTime testDate)
		{
			string dateString = testDate.ToString("yyyy_MM_dd_HH_mm_ss");
			Contact contactCreated = new Contact(_connection)
			{
				firstname = $"firstname_{dateString}",
				lastname = $"lastname_{dateString}",
			};
			return contactCreated;
		}
	}
}
