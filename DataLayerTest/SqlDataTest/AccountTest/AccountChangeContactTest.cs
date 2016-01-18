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
			Account accountInserted = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			accountInserted.Insert(_sqlConnection);

			AccountTest.AccountChangeTest accountChangeTest = new AccountTest.AccountChangeTest();
			accountChangeTest.TestFixtureSetUp();

			ExternalAccount externalAccountCreated = accountChangeTest.InsertExternalAccount(_sqlConnection);

			AccountChange accountChangeInserted = accountChangeTest.AccountChangeInsert(externalAccountCreated, accountInserted, DateTime.Now);
			accountChangeInserted.Insert();

			Contact contactInserted = new ContactTest.ContactTest().ContactInsert(_sqlConnection);

			AccountChangeContact accountChangeContact = new AccountChangeContact(accountChangeInserted.Id, contactInserted.Id);
			accountChangeContact.Insert(_sqlConnection);

			List<Contact> contacts = Contact.ReadContactsFromAccountChangeContact(_sqlConnection, accountChangeInserted.Id);
			accountChangeContact.Delete(_sqlConnection);

			Assert.AreEqual(contactInserted.Id, contacts.Single().Id);
		}

		[Test]
		public void Delete()
		{
			Account accountInserted = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			accountInserted.Insert(_sqlConnection);

			AccountTest.AccountChangeTest accountChangeTest = new AccountTest.AccountChangeTest();
            accountChangeTest.TestFixtureSetUp();

			ExternalAccount externalAccountCreated = accountChangeTest.InsertExternalAccount(_sqlConnection);

			AccountChange accountChangeInserted = accountChangeTest.AccountChangeInsert(externalAccountCreated, accountInserted, DateTime.Now);
			accountChangeInserted.Insert();

			Contact contactInserted = new ContactTest.ContactTest().ContactInsert(_sqlConnection);

			AccountChangeContact accountChangeContact = new AccountChangeContact(accountChangeInserted.Id, contactInserted.Id);
			accountChangeContact.Insert(_sqlConnection);

			accountChangeContact.Delete(_sqlConnection);
			List<Contact> contacts = Contact.ReadContactsFromAccountChangeContact(_sqlConnection, accountChangeInserted.Id);

			Assert.IsFalse(contacts.Any());
		}
	}
}
