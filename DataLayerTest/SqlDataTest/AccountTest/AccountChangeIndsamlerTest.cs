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
	public class AccountChangeIndsamlerTest
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

			AccountChangeIndsamler accountChangeIndsamler = new AccountChangeIndsamler(accountChangeInserted.Id, contactInserted.Id);
			accountChangeIndsamler.Insert(_sqlConnection);

			List<Contact> contacts = Contact.ReadContactsFromAccountChangeIndsamler(_sqlConnection, accountChangeInserted.Id);
			accountChangeIndsamler.Delete(_sqlConnection);

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

			AccountChangeIndsamler accountChangeIndsamler = new AccountChangeIndsamler(accountChangeInserted.Id, contactInserted.Id);
			accountChangeIndsamler.Insert(_sqlConnection);

			accountChangeIndsamler.Delete(_sqlConnection);
			List<Contact> contacts = Contact.ReadContactsFromAccountChangeIndsamler(_sqlConnection, accountChangeInserted.Id);

			Assert.IsFalse(contacts.Any());
		}
	}
}
