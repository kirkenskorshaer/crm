using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class LogTest
	{
		private MongoConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.DropDatabase();
		}

		[Test]
		public void ReadLatestTest()
		{
			Log.Write(_connection, "test1", string.Empty, Config.LogLevelEnum.OptionMessage);
			Log.Write(_connection, "test2", string.Empty, Config.LogLevelEnum.OptionMessage);
			Log.Write(_connection, "test3", string.Empty, Config.LogLevelEnum.OptionMessage);

			Log log = Log.ReadLatest(_connection);

			Assert.AreEqual("test3", log.Message);
		}
	}
}
