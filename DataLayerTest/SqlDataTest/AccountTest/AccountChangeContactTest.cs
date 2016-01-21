using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Account;
using DataLayer.SqlData.Contact;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.ChangeContactTest
{
	[TestFixture]
	public class AccountChangeContactTest
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
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeContact();

			List<Contact> contacts = Contact.ReadContactsFromAccountChangeContact(_sqlConnection, arrangedData.AccountChange.Id);
			arrangedData.AccountChangeContact.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.Contact.Id, contacts.Single().Id);
		}

		[Test]
		public void Delete()
		{
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeContact();

			arrangedData.AccountChangeContact.Delete(_sqlConnection);
			List<Contact> contacts = Contact.ReadContactsFromAccountChangeContact(_sqlConnection, arrangedData.AccountChange.Id);

			Assert.IsFalse(contacts.Any());
		}

		private DatabaseArrangeResponse ArrangeAccountChangeContact()
		{
			DatabaseArrangeResponse response = new DatabaseArrangeResponse();

			response.Account = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			response.Account.Insert(_sqlConnection);

			AccountTest.AccountChangeTest accountChangeTest = new AccountTest.AccountChangeTest();
			accountChangeTest.TestFixtureSetUp();

			response.ExternalAccount = accountChangeTest.InsertExternalAccount(_sqlConnection);

			response.AccountChange = accountChangeTest.AccountChangeInsert(response.ExternalAccount, response.Account, DateTime.Now);
			response.AccountChange.Insert();

			response.Contact = new ContactTest.ContactTest().ContactInsert(_sqlConnection);

			response.AccountChangeContact = new AccountChangeContact(response.AccountChange.Id, response.Contact.Id);
			response.AccountChangeContact.Insert(_sqlConnection);

			return response;
		}
	}
}
