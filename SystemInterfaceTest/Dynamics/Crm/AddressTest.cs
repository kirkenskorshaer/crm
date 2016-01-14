using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class AddressTest
	{
		private DynamicsCrmConnection _connection;

		[SetUp]
		public void SetUp()
		{
			MongoConnection connection = MongoConnection.GetConnection("test");
			UrlLogin login = UrlLogin.GetUrlLogin(connection, "test");
			_connection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);
		}

		[Test]
		[Ignore]
		public void GetAllAttributeNamesTest()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = new ContactTest().CreateTestContact(DateTime.Now);
			contactInserted.Insert();

			Address addressInserted = Address.Read(_connection, contactInserted.address1_addressid);

			List<string> attributeNames = Address.GetAllAttributeNames(_connection, addressInserted.AddressId);

			contactInserted.Delete();

			Assert.True(attributeNames.Count > 10);
			Assert.True(attributeNames.Any(name => name == "Guid customeraddressid"));

			attributeNames.ForEach(name => Console.Out.WriteLine(name));
		}

		[Test]
		public void UpdateUpdatesData()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = new ContactTest().CreateTestContact(DateTime.Now);
			contactInserted.address1_line1 = "test";
			contactInserted.Insert();

			Address addressInserted = Address.Read(_connection, contactInserted.address1_addressid);
			addressInserted.line1 = "test2";

			addressInserted.Update(_connection);

			Address addressRead = Address.Read(_connection, addressInserted.AddressId);
			contactInserted.Delete();

			Assert.AreEqual(addressInserted.line1, addressRead.line1);
		}
	}
}
