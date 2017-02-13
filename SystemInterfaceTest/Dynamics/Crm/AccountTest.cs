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
	public class AccountTest : TestBase
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

		[Test]
		public void AccountIndsamlerCanBeCounted()
		{
			Account accountInserted = new Account(_connection);
			string name = $"testnavn_{Guid.NewGuid()}";
			accountInserted.name = name;

			accountInserted.Insert();

			Contact contactInserted1 = CreateTestContact();
			contactInserted1.Insert();

			Contact contactInserted2 = CreateTestContact();
			contactInserted2.Insert();

			accountInserted.SynchronizeIndsamlere(new List<Contact> { contactInserted1, contactInserted2 });

			int indsamlingshjaelpere = accountInserted.CountIndsamlingsHjaelper();

			accountInserted.Delete();
			contactInserted1.Delete();
			contactInserted2.Delete();

			Assert.AreEqual(2, indsamlingshjaelpere);
		}

		[Test]
		[Ignore("")]
		public void GetIndsamlingsSted()
		{
			Materiale materiale = Materiale.ReadCalculationNeed(_dynamicsCrmConnection, _config.GetResourcePath);
			Guid businessId = materiale.owningbusinessunitGuid.Value;

			PagingInformation pagingInformation = new PagingInformation();

			int lastAccountCount = -1;
			int total = 0;

			while (lastAccountCount != 0)
			{
				List<Account> accounts = Account.GetIndsamlingsSted(_dynamicsCrmConnection, 15, businessId, _config.GetResourcePath, pagingInformation);
				lastAccountCount = accounts.Count;
				total += lastAccountCount;
				accounts.ForEach(account => Console.Out.WriteLine(account.Id));
				//Console.Out.Write($" {lastAccountCount} ");
			}
			Console.Out.WriteLine("-------");
			Console.Out.WriteLine(total);
		}
	}
}
