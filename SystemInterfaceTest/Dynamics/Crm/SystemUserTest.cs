using NUnit.Framework;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class SystemUserTest : TestBase
	{
		[Test]
		public void ContactIsCreatedByConnectingUser()
		{
			Contact contactInserted = CreateTestContact();
			contactInserted.Insert();

			Contact contactRead = Contact.Read(_dynamicsCrmConnection, contactInserted.Id);

			contactInserted.Delete();

			SystemUser user = SystemUser.Read(_dynamicsCrmConnection, contactRead.modifiedbyGuid.Value);

			Assert.AreEqual("KAD\\" + _urlLogin.Username, user.domainname);
		}

		[Test]
		public void UserCanBeRetreivedByDomainName()
		{
			string domainName = "KAD\\" + _urlLogin.Username;
            SystemUser user = SystemUser.ReadByDomainname(_dynamicsCrmConnection, domainName);

			Assert.IsNotNull(user);
		}
	}
}
