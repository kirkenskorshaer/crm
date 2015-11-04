using System;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System.Collections.Generic;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class UrlLoginTest
	{
		private MongoConnection _connection;

		private List<UrlLogin> _createdUrlLogins;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();

			_createdUrlLogins = new List<UrlLogin>();
		}

		[TearDown]
		public void TearDown()
		{
			_connection.CleanDatabase();

			_createdUrlLogins.ForEach(urlLogin => urlLogin.Delete(_connection));
		}

		[Test]
		public void GetFirstTest()
		{
			_connection.CleanDatabase();
			UrlLogin urlLoginInserted = InsertUrlLogin();

			UrlLogin urlLoginReturned = UrlLogin.GetFirst(_connection);

			bool returnedIsInserted = urlLoginInserted.Url == urlLoginReturned.Url;

			Assert.True(returnedIsInserted);
		}

		[Test]
		public void GetUrlLoginRetreivesUrlLogin()
		{
			UrlLogin urlLoginOrigin = InsertUrlLogin();

			UrlLogin urlLoginRestored = UrlLogin.GetUrlLogin(_connection, urlLoginOrigin.UrlName);

			Assert.AreEqual(urlLoginOrigin.Password, urlLoginRestored.Password);
		}

		[Test]
		public void ExistsReturnFalseIfNoUrlLoginExists()
		{
			_connection.CleanDatabase();

			string name = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			bool exists = UrlLogin.Exists(_connection, name);

			Assert.False(exists);
		}

		[Test]
		public void ExistsReturnFalseIfOtherUrlLoginExists()
		{
			_connection.CleanDatabase();

			InsertUrlLogin();
			string name = DateTime.Now.ToString("_yyyy-MM-dd HH:mm:ss");

			bool exists = UrlLogin.Exists(_connection, name);

			Assert.False(exists);
		}

		[Test]
		public void ExistsReturnTrueIfUrlLoginExists()
		{
			UrlLogin urlLogin = InsertUrlLogin();

			bool exists = UrlLogin.Exists(_connection, urlLogin.UrlName);

			Assert.True(exists);
		}

		private UrlLogin InsertUrlLogin()
		{
			string name = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string url = "www.test.com";

			UrlLogin urlLogin;

			if (UrlLogin.Exists(_connection, name) == false)
			{
				urlLogin = new UrlLogin()
				{
					UrlName = name,
					Url = url,
					Password = "testPassword",
					Username = "testUsername",
				};

				urlLogin.Insert(_connection);

				_createdUrlLogins.Add(urlLogin);
			}
			else
			{
				urlLogin = UrlLogin.GetUrlLogin(_connection, name);
			}

			return urlLogin;
		}
	}
}
