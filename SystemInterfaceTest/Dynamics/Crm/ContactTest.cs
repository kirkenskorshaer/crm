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
			contactInserted1.Insert(_connection);
			Contact contactInserted2 = CreateTestContact(testDate);
			contactInserted2.Insert(_connection);

			List<Contact> contacts = Contact.ReadLatest(_connection, testDate);

			contactInserted1.Delete(_connection);
			contactInserted2.Delete(_connection);

			Assert.AreEqual(2, contacts.Count);
		}

		[Test]
		[Ignore]
		public void GetAllAttributeNamesTest()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			contactInserted.Insert(_connection);

			List<string> attributeNames = Contact.GetAllAttributeNames(_connection, contactInserted.ContactId);

			contactInserted.Delete(_connection);

			Assert.True(attributeNames.Count > 10);
			Assert.True(attributeNames.Any(name => name == "contactid"));

			attributeNames.ForEach(name => Console.Out.WriteLine(name));
		}

		[Test]
		public void InsertCreatesNewContact()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);

			contactInserted.Insert(_connection);
			List<Contact> contacts = Contact.ReadLatest(_connection, testDate);
			contactInserted.Delete(_connection);

			Assert.True(contacts.Any(contact => contact.ContactId == contactInserted.ContactId));
			Assert.AreNotEqual(Guid.Empty, contactInserted.ContactId);
		}

		[Test]
		public void DeleteRemovesContact()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);

			contactInserted.Insert(_connection);
			contactInserted.Delete(_connection);
			List<Contact> contacts = Contact.ReadLatest(_connection, testDate);

			Assert.False(contacts.Any(contact => contact.ContactId == contactInserted.ContactId));
			Assert.AreNotEqual(Guid.Empty, contactInserted.ContactId);
		}

		[Test]
		public void UpdateUpdatesData()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			string firstnameTest = "firstnameTest";

			contactInserted.Insert(_connection);
			contactInserted.Firstname = firstnameTest;

			contactInserted.Update(_connection);

			Contact contactRead = Contact.Read(_connection, contactInserted.ContactId);
			contactInserted.Delete(_connection);

			Assert.AreEqual(firstnameTest, contactRead.Firstname);
		}

		[Test]
		public void ContactIsCreatedAsActive()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(testDate);
			contactInserted.Insert(_connection);

			Contact contactRead = Contact.Read(_connection, contactInserted.ContactId);

			contactInserted.Delete(_connection);

			Assert.AreEqual(Contact.StateEnum.Active, contactRead.State);
		}

		private Contact CreateTestContact(DateTime testDate)
		{
			string dateString = testDate.ToString("yyyy_MM_dd_HH_mm_ss");
			Contact contactCreated = new Contact
			{
				Firstname = $"firstname_{dateString}",
				Lastname = $"lastname_{dateString}",
			};
			return contactCreated;
		}
	}
}
