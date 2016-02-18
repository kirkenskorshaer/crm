using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class AccountTest
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
		public void ReadIsSameAsInserted()
		{
			Account accountInserted = new Account(_connection);
			string name = $"testnavn_{Guid.NewGuid()}";
			accountInserted.name = name;

			accountInserted.Insert();

			Account accountRead = Account.Read(_connection, accountInserted.Id);

			accountInserted.Delete();

			Assert.AreEqual(name, accountRead.name);
		}

		[Test]
		public void EntityReferenceIsSet()
		{
			Contact relatedContact = new Contact(_connection);
			relatedContact.firstname = $"testnavn_{Guid.NewGuid()}";
			relatedContact.Insert();

			Account accountInserted = new Account(_connection);
			string name = $"testnavn_{Guid.NewGuid()}";
			accountInserted.name = name;
			accountInserted.bykoordinatorid = relatedContact.Id;

			accountInserted.Insert();

			Account accountRead = Account.Read(_connection, accountInserted.Id);

			accountInserted.Delete();
			relatedContact.Delete();

			Assert.AreEqual(relatedContact.Id, accountRead.bykoordinatorid);
		}

		[Test]
		public void kredsellerbyCanBeSet()
		{
			Account accountInserted = new Account(_connection);
			string name = $"testnavn_{Guid.NewGuid()}";
			accountInserted.name = name;
			accountInserted.kredsellerby = Account.kredsellerbyEnum.kreds;

			accountInserted.Insert();

			Account accountRead = Account.Read(_connection, accountInserted.Id);
			Account.kredsellerbyEnum? readKredsellerby = accountRead.kredsellerby;

			accountInserted.Delete();

			Assert.AreEqual(accountInserted.kredsellerby, readKredsellerby);
		}
    }
}
