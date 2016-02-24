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
	public class AddressTest : TestBase
	{
		[Test]
		[Ignore]
		public void GetAllAttributeNamesTest()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(DateTime.Now);
			contactInserted.Insert();

			Address addressInserted = Address.Read(_dynamicsCrmConnection, contactInserted.address1_addressid);

			List<string> attributeNames = Address.GetAllAttributeNames(_dynamicsCrmConnection, addressInserted.AddressId);

			contactInserted.Delete();

			Assert.True(attributeNames.Count > 10);
			Assert.True(attributeNames.Any(name => name == "Guid customeraddressid"));

			attributeNames.ForEach(name => Console.Out.WriteLine(name));
		}

		[Test]
		public void UpdateUpdatesData()
		{
			DateTime testDate = DateTime.Now;
			Contact contactInserted = CreateTestContact(DateTime.Now);
			contactInserted.address1_line1 = "test";
			contactInserted.Insert();

			Address addressInserted = Address.Read(_dynamicsCrmConnection, contactInserted.address1_addressid);
			addressInserted.line1 = "test2";

			addressInserted.Update(_dynamicsCrmConnection);

			Address addressRead = Address.Read(_dynamicsCrmConnection, addressInserted.AddressId);
			contactInserted.Delete();

			Assert.AreEqual(addressInserted.line1, addressRead.line1);
		}
	}
}
