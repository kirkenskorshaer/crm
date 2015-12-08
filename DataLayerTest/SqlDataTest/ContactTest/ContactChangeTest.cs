using DataLayer;
using DataLayer.SqlData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer.SqlData.Contact;
using System.Linq;

namespace DataLayerTest.SqlDataTest.ContactTest
{
	[TestFixture]
	public class ContactChangeTest : TestSqlBase
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

		internal ExternalContact InsertExternalContact(SqlConnection sqlConnection)
		{
			DateTime creationDate = DateTime.Now;

			ChangeProvider changeProvider = new ChangeProvider();
			changeProvider.Name = $"name_{Guid.NewGuid()}";

			changeProvider.Insert(sqlConnection);

			Guid changeProviderId = changeProvider.Id;

			ExternalContact createdExternalContact = new ExternalContact(sqlConnection, Guid.NewGuid(), changeProviderId);

			createdExternalContact.Insert();

			return createdExternalContact;
		}

		[TestCase(ContactChange.IdType.ChangeProviderId)]
		[TestCase(ContactChange.IdType.ContactChangeId)]
		[TestCase(ContactChange.IdType.ContactId)]
		[TestCase(ContactChange.IdType.ExternalContactId)]
		public void ReadReadsInserted(ContactChange.IdType idType)
		{
			ExternalContact externalContactCreated = InsertExternalContact(_sqlConnection);
			Contact contactCreated = InsertContact(_sqlConnection);
			DateTime createdTime = DateTime.Now;

			ContactChange contactChangeCreated = ContactChangeInsert(externalContactCreated, contactCreated, createdTime);

			contactChangeCreated.Insert();

			Guid id = Guid.Empty;
			switch (idType)
			{
				case ContactChange.IdType.ContactChangeId:
					id = contactChangeCreated.Id;
					break;
				case ContactChange.IdType.ContactId:
					id = contactCreated.Id;
					break;
				case ContactChange.IdType.ExternalContactId:
					id = externalContactCreated.ExternalContactId;
					break;
				case ContactChange.IdType.ChangeProviderId:
					id = externalContactCreated.ChangeProviderId;
					break;
				default:
					break;
			}

			List<ContactChange> contactChangesRead = ContactChange.Read(_sqlConnection, id, idType);

			contactChangeCreated.Delete(_sqlConnection);

			Assert.AreEqual(contactChangeCreated.Firstname, contactChangesRead.Single().Firstname);
			Assert.AreEqual(contactChangeCreated.Lastname, contactChangesRead.Single().Lastname);
		}

		internal ContactChange ContactChangeInsert(ExternalContact externalContactCreated, Contact contactCreated, DateTime createdTime)
		{
			return new ContactChange(_sqlConnection, contactCreated.Id, externalContactCreated.ExternalContactId, externalContactCreated.ChangeProviderId)
			{
				Firstname = $"name_{Guid.NewGuid()}",
				CreatedOn = createdTime,
				ModifiedOn = createdTime,
			};
		}

		[Test]
		public void GetContactsReturnsContacts()
		{
			ExternalContact externalContact1Created = InsertExternalContact(_sqlConnection);
			ExternalContact externalContact2Created = InsertExternalContact(_sqlConnection);

			Contact contact1Created = InsertContact(_sqlConnection);
			Contact contact2Created = InsertContact(_sqlConnection);

			DateTime creationDate = DateTime.Now;

			ContactChange contactChange1_1Created = new ContactChange(_sqlConnection, contact1Created.Id, externalContact1Created.ExternalContactId, externalContact1Created.ChangeProviderId)
			{
				Firstname = "test",
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			contactChange1_1Created.Insert();

			ContactChange contactChange1_2Created = new ContactChange(_sqlConnection, contact1Created.Id, externalContact2Created.ExternalContactId, externalContact2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			contactChange1_2Created.Insert();

			ContactChange contactChange2Created = new ContactChange(_sqlConnection, contact2Created.Id, externalContact2Created.ExternalContactId, externalContact2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			contactChange2Created.Insert();

			List<Contact> contactsChangedByExternalContact2 = ContactChange.GetContacts(_sqlConnection, externalContact2Created.ExternalContactId);

			contactChange1_1Created.Delete(_sqlConnection);
			contactChange1_2Created.Delete(_sqlConnection);
			contactChange2Created.Delete(_sqlConnection);

			Assert.True(contactsChangedByExternalContact2.Any(contact => contact.Id == contact1Created.Id));
			Assert.True(contactsChangedByExternalContact2.Any(contact => contact.Id == contact2Created.Id));
		}

		[Test]
		public void GetExternalContacts()
		{
			ExternalContact externalContact1Created = InsertExternalContact(_sqlConnection);
			ExternalContact externalContact2Created = InsertExternalContact(_sqlConnection);

			Contact contact1Created = InsertContact(_sqlConnection);
			Contact contact2Created = InsertContact(_sqlConnection);

			DateTime creationDate = DateTime.Now;

			ContactChange contactChange1_1Created = new ContactChange(_sqlConnection, contact1Created.Id, externalContact1Created.ExternalContactId, externalContact1Created.ChangeProviderId)
			{
				Firstname = "test",
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			contactChange1_1Created.Insert();

			ContactChange contactChange1_2Created = new ContactChange(_sqlConnection, contact1Created.Id, externalContact2Created.ExternalContactId, externalContact2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			contactChange1_2Created.Insert();

			ContactChange contactChange2Created = new ContactChange(_sqlConnection, contact2Created.Id, externalContact2Created.ExternalContactId, externalContact2Created.ChangeProviderId)
			{
				CreatedOn = creationDate,
				ModifiedOn = creationDate,
			};
			contactChange2Created.Insert();

			List<ExternalContact> externalContactsChangingContact1 = ContactChange.GetExternalContacts(_sqlConnection, contact1Created.Id);

			contactChange1_1Created.Delete(_sqlConnection);
			contactChange1_2Created.Delete(_sqlConnection);
			contactChange2Created.Delete(_sqlConnection);

			Assert.True(externalContactsChangingContact1.Any(externalContact => externalContact.ExternalContactId == externalContact1Created.ExternalContactId && externalContact.ChangeProviderId == externalContact1Created.ChangeProviderId));
			Assert.True(externalContactsChangingContact1.Any(externalContact => externalContact.ExternalContactId == externalContact2Created.ExternalContactId && externalContact.ChangeProviderId == externalContact2Created.ChangeProviderId));
		}
	}
}
