using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DataLayer;
using DataLayer.SqlData;
using NUnit.Framework;
using System.Linq;

namespace DataLayerTest.SqlData
{
	[TestFixture]
	public class ContactTest
	{
		private MongoConnection _mongoConnection;

		[SetUp]
		public void SetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
		}

		[Test]
		public void ReadLatestTest()
		{
			SqlConnection sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "testMssql");
			if (Utilities.GetExistingColumns(sqlConnection, "contact").Any())
			{
				Utilities.DropTable(sqlConnection, "contact");
			}

			Contact.MaintainTable(sqlConnection);

			DateTime creationDate = DateTime.Now;

			Contact createdContact = new Contact
			{
				Firstname = "FirstnameTest",
				Lastname = "LastNameTest",
				ModifiedOn = creationDate,
				CreatedOn = creationDate,
			};

			createdContact.Insert(sqlConnection);

			List<Contact> contacts = Contact.ReadLatest(sqlConnection, creationDate.AddSeconds(-1));

			Assert.AreEqual(1, contacts.Count);
		}
	}
}
