using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class AbstractCrmTest : TestBase
	{
		[Test]
		public void CreateFromContentCanCreateFromInvalidContent()
		{
			Dictionary<string, string> contactDefinition = GetContactDefinition();

			Contact contact = Contact.Create(_dynamicsCrmConnection, contactDefinition);

			Assert.AreEqual(contactDefinition["firstname"], contact.firstname);
		}

		[Test]
		public void AssignAssignsContactToTeam()
		{
			Contact contact = CreateAndInsertContact();

			Guid teamId = Team.GetIdByNamed(_dynamicsCrmConnection, "Landsindsamling").Value;

			contact.owner = teamId;
			bool assignResult = contact.Assign();

			List<Contact> contacts = Contact.ReadFromFetchXml(_dynamicsCrmConnection, new List<string>() { "firstname", "ownerid" }, new Dictionary<string, string>() { { "contactid", contact.Id.ToString() } });

			Contact readContact = contacts.Single();

			Assert.IsTrue(assignResult);
			Assert.AreEqual(contact.firstname, readContact.firstname);
			Assert.AreEqual(teamId, readContact.owner);

			contact.Delete();
		}

		[Test]
		public void AssignAssignsContactToUser()
		{
			Contact contact = CreateAndInsertContact();

			SystemUser user = SystemUser.ReadByDomainname(_dynamicsCrmConnection, "KAD\\Svend");
			Guid userId = user.Id;

			contact.owner = userId;
			bool assignResult = contact.Assign();

			List<Contact> contacts = Contact.ReadFromFetchXml(_dynamicsCrmConnection, new List<string>() { "firstname", "ownerid" }, new Dictionary<string, string>() { { "contactid", contact.Id.ToString() } });

			Contact readContact = contacts.Single();

			Assert.IsTrue(assignResult);
			Assert.AreEqual(contact.firstname, readContact.firstname);
			Assert.AreEqual(userId, readContact.owner);

			contact.Delete();
		}

		[Test]
		public void AssignReturnsFalseIfFailed()
		{
			Contact contact = CreateAndInsertContact();

			Guid teamId = Team.GetIdByNamed(_dynamicsCrmConnection, "Kirkens Korshær").Value;

			contact.owner = teamId;
			bool assignResult = contact.Assign();

			List<Contact> contacts = Contact.ReadFromFetchXml(_dynamicsCrmConnection, new List<string>() { "firstname", "ownerid" }, new Dictionary<string, string>() { { "contactid", contact.Id.ToString() } });

			Assert.IsFalse(assignResult);

			contact.Delete();
		}

		private static Dictionary<string, string> GetContactDefinition()
		{
			string firstname = $"firstname {Guid.NewGuid()}";
			Dictionary<string, string> contactDefinition = new Dictionary<string, string>()
			{
				{ "contactid", "test" },
				{ "nonExistingFieldOrPropertyName", "test" },
				{ "firstname", firstname },
			};

			return contactDefinition;
		}

		private Contact CreateAndInsertContact()
		{
			Dictionary<string, string> contactDefinition = GetContactDefinition();

			Contact contact = Contact.Create(_dynamicsCrmConnection, contactDefinition);
			contact.InsertWithoutRead();

			return contact;
		}
	}
}
