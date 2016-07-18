using NUnit.Framework;
using System;
using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class AbstractCrmTest : TestBase
	{
		[Test]
		public void CreateFromContentCanCreateFromInvalidContent()
		{
			string firstname = $"firstname {Guid.NewGuid()}";

			Dictionary<string, string> contactDefinition = new Dictionary<string, string>()
			{
				{ "contactid", "test" },
				{ "nonExistingFieldOrPropertyName", "test" },
				{ "firstname", firstname },
			};

			Contact contact = Contact.Create(_dynamicsCrmConnection, contactDefinition);

			Assert.AreEqual(firstname, contact.firstname);
		}
	}
}
