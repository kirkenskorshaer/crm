using DataLayer;
using DataLayer.SqlData;
using DataLayer.SqlData.Account;
using DataLayer.SqlData.Contact;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayerTest.SqlDataTest.ContactTest
{
	[TestFixture]
	public class AccountIndsamlerTest
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

			Contact contactInserted = new ContactTest().ContactInsert(_sqlConnection);

			AccountIndsamler accountIndsamler = new AccountIndsamler(accountInserted.Id, contactInserted.Id);
			accountIndsamler.Insert(_sqlConnection);

			List<Contact> contacts = Contact.ReadContactsFromAccountIndsamler(_sqlConnection, accountInserted.Id);
			accountIndsamler.Delete(_sqlConnection);

			Assert.AreEqual(contactInserted.Id, contacts.Single().Id);
		}

		[Test]
		public void Delete()
		{
			Account accountInserted = new AccountTest.AccountTest().AccountInsert(_sqlConnection);
			accountInserted.Insert(_sqlConnection);

			Contact contactInserted = new ContactTest().ContactInsert(_sqlConnection);

			AccountIndsamler accountIndsamler = new AccountIndsamler(accountInserted.Id, contactInserted.Id);
			accountIndsamler.Insert(_sqlConnection);

			accountIndsamler.Delete(_sqlConnection);
			List<Contact> contacts = Contact.ReadContactsFromAccountIndsamler(_sqlConnection, accountInserted.Id);

			Assert.IsFalse(contacts.Any());
		}
	}
}
