using System;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System.Collections.Generic;

namespace DataLayerTest.MongoDataTest
{
	public class SqlConnectionStringTest
	{
		private MongoConnection _connection;

		private List<SqlConnectionString> _createdSqlConnectionStrings;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();

			_createdSqlConnectionStrings = new List<SqlConnectionString>();
		}

		[TearDown]
		public void TearDown()
		{
			_connection.CleanDatabase();

			_createdSqlConnectionStrings.ForEach(sqlConnectionString => sqlConnectionString.Delete(_connection));
		}

		[Test]
		public void GetSqlConnectionStringRetreivesSqlConnectionString()
		{
			SqlConnectionString sqlConnectionStringOrigin = InsertSqlConnectionString();

			SqlConnectionString sqlConnectionStringRestored = SqlConnectionString.GetSqlConnectionString(_connection, sqlConnectionStringOrigin.ConnectionName);

			Assert.AreEqual(sqlConnectionStringOrigin.ConnectionString, sqlConnectionStringRestored.ConnectionString);
		}

		[Test]
		public void ExistsReturnFalseIfNoSqlConnectionStringExists()
		{
			_connection.CleanDatabase();

			string name = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			bool exists = SqlConnectionString.Exists(_connection, name);

			Assert.False(exists);
		}

		[Test]
		public void ExistsReturnFalseIfOtherSqlConnectionStringExists()
		{
			_connection.CleanDatabase();

			InsertSqlConnectionString();
			string name = DateTime.Now.ToString("_yyyy-MM-dd HH:mm:ss");

			bool exists = SqlConnectionString.Exists(_connection, name);

			Assert.False(exists);
		}

		[Test]
		public void ExistsReturnTrueIfSqlConnectionStringExists()
		{
			SqlConnectionString sqlConnectionString = InsertSqlConnectionString();

			bool exists = SqlConnectionString.Exists(_connection, sqlConnectionString.ConnectionName);

			Assert.True(exists);
		}

		private SqlConnectionString InsertSqlConnectionString()
		{
			string name = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string connectionString = "connectionString" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			SqlConnectionString sqlConnectionString;

			if (SqlConnectionString.Exists(_connection, name) == false)
			{
				sqlConnectionString = new SqlConnectionString()
				{
					ConnectionName = name,
					ConnectionString = connectionString,
				};

				sqlConnectionString.Insert(_connection);

				_createdSqlConnectionStrings.Add(sqlConnectionString);
			}
			else
			{
				sqlConnectionString = SqlConnectionString.GetSqlConnectionString(_connection, name);
			}

			return sqlConnectionString;
		}
	}
}
