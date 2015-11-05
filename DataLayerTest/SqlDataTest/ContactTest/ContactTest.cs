using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer.SqlData;
using NUnit.Framework;
using System.Linq;
using DataLayer;
using DataLayer.SqlData.Contact;

namespace DataLayerTest.SqlDataTest.ContactTest
{
	[TestFixture]
	public class ContactTest
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
			if (Utilities.GetExistingColumns(_sqlConnection, "Contact").Any())
			{
				if (Utilities.GetExistingColumns(_sqlConnection, "ContactChange").Any())
				{
					Utilities.DropTable(_sqlConnection, "ContactChange");
				}
				Utilities.DropTable(_sqlConnection, "Contact");
			}
			Contact.MaintainTable(_sqlConnection);
		}

		[Test]
		public void DeleteTest()
		{
			Contact createdContact = ContactInsert(_sqlConnection);

			createdContact.Delete(_sqlConnection);

			List<Contact> contacts = Contact.ReadLatest(_sqlConnection, createdContact.CreatedOn.AddSeconds(-1));
			Assert.AreEqual(0, contacts.Count);
		}

		[Test]
		public void ReadLatestTest()
		{
			Contact createdContact = ContactInsert(_sqlConnection);

			List<Contact> contacts = Contact.ReadLatest(_sqlConnection, createdContact.CreatedOn.AddSeconds(-1));

			Assert.AreEqual(1, contacts.Count);
		}

		[Test]
		public void ReadReadsCorrectContact()
		{
			Contact createdContact1 = ContactInsert(_sqlConnection);
			Contact createdContact2 = ContactInsert(_sqlConnection);
			Contact createdContact3 = ContactInsert(_sqlConnection);
			Contact createdContact4 = ContactInsert(_sqlConnection);
			Contact createdContact5 = ContactInsert(_sqlConnection);

			Contact contact3Read = Contact.Read(_sqlConnection, createdContact3.Id);

			createdContact1.Delete(_sqlConnection);
			createdContact2.Delete(_sqlConnection);
			createdContact3.Delete(_sqlConnection);
			createdContact4.Delete(_sqlConnection);
			createdContact5.Delete(_sqlConnection);

			Assert.AreEqual(createdContact3.Firstname, contact3Read.Firstname);
		}

		[Test]
		public void ContactCanBeRestoredWithoutOptionalColumn()
		{
			Contact createdContact = ContactInsertWithoutLastname(_sqlConnection);

			Contact contactRead = Contact.Read(_sqlConnection, createdContact.Id);

			createdContact.Delete(_sqlConnection);

			Assert.AreEqual(createdContact.Firstname, contactRead.Firstname);
		}

		[Test]
		public void ContactCanBeUpdated()
		{
			Contact createdContact = ContactInsert(_sqlConnection);
			createdContact.Firstname = "newFirstName";

			createdContact.Update(_sqlConnection);

			Contact contactRead = Contact.Read(_sqlConnection, createdContact.Id);

			createdContact.Delete(_sqlConnection);

			Assert.AreEqual(createdContact.Firstname, contactRead.Firstname);
		}

		private Contact ContactInsertWithoutLastname(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			Contact createdContact = new Contact
			{
				Firstname = $"Firstname_{Guid.NewGuid()}",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			createdContact.Insert(sqlConnection);
			return createdContact;
		}

		private Contact ContactInsert(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			Contact createdContact = new Contact
			{
				Firstname = $"Firstname_{Guid.NewGuid()}",
				Lastname = "LastNameTest",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			createdContact.Insert(sqlConnection);
			return createdContact;
		}
	}
}
