﻿using DataLayer;
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
	}
}