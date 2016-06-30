using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class StaticCrmTest : TestBase
	{
		[Test]
		public void GetAllAttributeNames()
		{
			List<string> attributeNames = StaticCrm.GetAllAttributeNames(_dynamicsCrmConnection, typeof(Annotation));

			Assert.Greater(attributeNames.Count, 3);
		}

		[Test]
		public void ExistsReturnsTrueWhenContactExists()
		{
			Contact contact = CreateTestContact();
			contact.emailaddress1 = $"email {Guid.NewGuid()}";
			contact.firstname = $"firstname {Guid.NewGuid()}";
			contact.Insert();

			bool exists = StaticCrm.Exists<Contact>(_dynamicsCrmConnection, new Dictionary<string, string>()
			{
				{ "emailaddress1",contact.emailaddress1 },
				{ "firstname",contact.firstname },
			});

			contact.Delete();

			Assert.True(exists);
		}

		[Test]
		public void ExistsReturnsFalseWhenContactDoesNotExists()
		{
			Contact contact = CreateTestContact();
			contact.emailaddress1 = $"email {Guid.NewGuid()}";
			contact.firstname = $"firstname {Guid.NewGuid()}";
			contact.Insert();

			bool exists = StaticCrm.Exists<Contact>(_dynamicsCrmConnection, new Dictionary<string, string>()
			{
				{ "emailaddress1",contact.emailaddress1 },
				{ "firstname",$"firstname {Guid.NewGuid()}" },
			});

			contact.Delete();

			Assert.False(exists);
		}

		[Test]
		public void ReadFromFetchXml()
		{
			Contact contact = CreateTestContact();
			contact.middlename = $"middlename {Guid.NewGuid()}";
			contact.emailaddress1 = $"email {Guid.NewGuid()}";

			contact.Insert();

			List<string> fields = new List<string>() { "firstname", "emailaddress1" };
			Dictionary<string, string> keyContent = new Dictionary<string, string>()
			{
				{ "middlename", contact.middlename },
				{ "emailaddress1", contact.emailaddress1 }
			};

			Contact contactRead = Contact.ReadFromFetchXml(_dynamicsCrmConnection, fields, keyContent).Single();
			contact.Delete();

			Assert.AreEqual(contact.firstname, contactRead.firstname);
			Assert.IsNull(contactRead.lastname);
		}
	}
}
