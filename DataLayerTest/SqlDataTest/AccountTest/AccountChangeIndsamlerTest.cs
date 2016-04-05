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
			SqlUtilities.RecreateAllTables(_sqlConnection);
		}

		[Test]
		public void Insert()
		{
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeIndsamler();

			List<Contact> contacts = Contact.ReadContactsFromAccountChangeIndsamler(_sqlConnection, arrangedData.AccountChange.Id);
			arrangedData.AccountChangeIndsamler.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.Contact.Id, contacts.Single().Id);
		}

		[Test]
		public void Delete()
		{
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeIndsamler();

			arrangedData.AccountChangeIndsamler.Delete(_sqlConnection);
			List<Contact> contacts = Contact.ReadContactsFromAccountChangeIndsamler(_sqlConnection, arrangedData.AccountChange.Id);

			Assert.IsFalse(contacts.Any());
		}

		[Test]
		public void ReadFromAccountChangeId()
		{
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeIndsamler();

			List<AccountChangeIndsamler> accountChangeIndsamlere = AccountChangeIndsamler.ReadFromAccountChangeId(_sqlConnection, arrangedData.AccountChange.Id);
			arrangedData.AccountChangeIndsamler.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.AccountChangeIndsamler, accountChangeIndsamlere.Single());
		}

		[Test]
		public void ReadFromContactChangeId()
		{
			DatabaseArrangeResponse arrangedData = ArrangeAccountChangeIndsamler();

			List<AccountChangeIndsamler> accountChangeIndsamlere = AccountChangeIndsamler.ReadFromContactId(_sqlConnection, arrangedData.Contact.Id);
			arrangedData.AccountChangeIndsamler.Delete(_sqlConnection);

			Assert.AreEqual(arrangedData.AccountChangeIndsamler, accountChangeIndsamlere.Single());
		}

		private DatabaseArrangeResponse ArrangeAccountChangeIndsamler()
		{
			DatabaseArrangeResponse response = new DatabaseArrangeResponse();

			response.Account = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			response.Account.Insert(_sqlConnection);

			AccountTest.AccountChangeTest accountChangeTest = new AccountTest.AccountChangeTest();
			accountChangeTest.TestFixtureSetUp();

			response.ExternalAccount = accountChangeTest.InsertExternalAccount(_sqlConnection, response.Account.Id);

			response.AccountChange = accountChangeTest.AccountChangeInsert(response.ExternalAccount, response.Account, DateTime.Now);
			response.AccountChange.Insert();

			response.Contact = new ContactTest.ContactTest().ContactInsert(_sqlConnection);

			response.AccountChangeIndsamler = new AccountChangeIndsamler(response.AccountChange.Id, response.Contact.Id);
			response.AccountChangeIndsamler.Insert(_sqlConnection);

			return response;
		}
	}
}
