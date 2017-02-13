using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayerTest
{
	[TestFixture]
	public class MongoConnectionTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();
		}

		[Test]
		public void ReadAsDictionaries()
		{
			string messageValue = "test1";

			Log.Write(_connection, messageValue, typeof(MongoConnectionTest), string.Empty, Config.LogLevelEnum.OptionMessage);

			List<Dictionary<string, object>> readList = _connection.ReadAsDictionaries(typeof(Log).Name);

			Assert.AreEqual(readList.Single()["Message"], messageValue);
		}
	}
}
