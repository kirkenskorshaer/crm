using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer.SqlData;
using NUnit.Framework;
using System.Linq;
using DataLayer;
using DataLayer.SqlData.Contact;
using System.Data.SqlTypes;
using DataLayer.MongoData;

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
			SqlUtilities.RecreateAllTables(_sqlConnection);
		}

		[Test]
		public void DeleteTest()
		{
			Contact createdContact = ContactInsert(_sqlConnection);

			createdContact.Delete(_sqlConnection);

			List<Contact> contacts = Contact.ReadLatest(_sqlConnection, createdContact.createdon.AddSeconds(-1));
			Assert.AreEqual(0, contacts.Count);
		}

		[Test]
		public void ReadLatestTest()
		{
			Contact createdContact = ContactInsert(_sqlConnection);

			List<Contact> contacts = Contact.ReadLatest(_sqlConnection, createdContact.createdon.AddSeconds(-1));

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

			Assert.AreEqual(createdContact3.firstname, contact3Read.firstname);
		}

		[Test]
		public void ContactCanBeRestoredWithoutOptionalColumn()
		{
			Contact createdContact = ContactInsertWithoutLastname(_sqlConnection);

			Contact contactRead = Contact.Read(_sqlConnection, createdContact.Id);

			createdContact.Delete(_sqlConnection);

			Assert.AreEqual(createdContact.firstname, contactRead.firstname);
		}

		[Test]
		public void ContactCanBeUpdated()
		{
			Contact createdContact = ContactInsert(_sqlConnection);
			createdContact.firstname = "newFirstName";

			createdContact.Update(_sqlConnection);

			Contact contactRead = Contact.Read(_sqlConnection, createdContact.Id);

			createdContact.Delete(_sqlConnection);

			Assert.AreEqual(createdContact.firstname, contactRead.firstname);
		}

		[Test]
		public void ReadNextByIdReturnsNullWhenThereIsNoContacts()
		{
			Contact readContact = Contact.ReadNextById(_sqlConnection, Guid.Empty);

			Assert.IsNull(readContact);
		}

		[Test]
		public void ReadNextByIdReturnsContactsInOrder()
		{
			Contact createdContact1 = ContactInsert(_sqlConnection);
			Contact createdContact2 = ContactInsert(_sqlConnection);
			Contact createdContact3 = ContactInsert(_sqlConnection);
			Contact createdContact4 = ContactInsert(_sqlConnection);

			Contact readContact1 = Contact.ReadNextById(_sqlConnection, Guid.Empty);
			Contact readContact2 = Contact.ReadNextById(_sqlConnection, readContact1.Id);
			Contact readContact3 = Contact.ReadNextById(_sqlConnection, readContact2.Id);
			Contact readContact4 = Contact.ReadNextById(_sqlConnection, readContact3.Id);
			Contact readContact5 = Contact.ReadNextById(_sqlConnection, readContact4.Id);

			SqlGuid[] sortedGuids = new List<Contact>() { createdContact1, createdContact2, createdContact3, createdContact4 }.Select(contact => new SqlGuid(contact.Id)).OrderBy(sqlGuid => sqlGuid).ToArray();

			Assert.AreEqual(sortedGuids[0], new SqlGuid(readContact1.Id));
			Assert.AreEqual(sortedGuids[1], new SqlGuid(readContact2.Id));
			Assert.AreEqual(sortedGuids[2], new SqlGuid(readContact3.Id));
			Assert.AreEqual(sortedGuids[3], new SqlGuid(readContact4.Id));
			Assert.AreEqual(sortedGuids[0], new SqlGuid(readContact5.Id));
		}

		[Test]
		public void InsertCreatesProgress()
		{
			Contact createdContact1 = ContactInsert(_sqlConnection, true);

			bool progressExists = Progress.Exists(_mongoConnection, "Contact", createdContact1.Id);

			Assert.IsTrue(progressExists);
		}

		private Contact ContactInsertWithoutLastname(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			Contact createdContact = new Contact
			{
				firstname = $"Firstname_{Guid.NewGuid()}",
				modifiedon = creationDate,
				createdon = creationDate,
			};

			createdContact.Insert(sqlConnection);
			return createdContact;
		}

		internal Contact ContactInsert(SqlConnection sqlConnection, bool useMongoConnection = false)
		{
			DateTime creationDate = DateTime.Now;

			Contact createdContact = new Contact
			{
				firstname = $"Firstname_{Guid.NewGuid()}",
				lastname = "LastNameTest",
				modifiedon = creationDate,
				createdon = creationDate,
			};

			if (useMongoConnection)
			{
				createdContact.Insert(sqlConnection, _mongoConnection);
			}
			else
			{
				createdContact.Insert(sqlConnection);
			}
			return createdContact;
		}
	}
}
