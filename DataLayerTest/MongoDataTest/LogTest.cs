using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
			_connection.CleanDatabase();
		}

		[Test]
		public void ReadLatestTest()
		{
			Log.Write(_connection, "test1", typeof(LogTest), string.Empty, Config.LogLevelEnum.OptionMessage);
			Thread.Sleep(50);
			Log.Write(_connection, "test2", typeof(LogTest), string.Empty, Config.LogLevelEnum.OptionMessage);
			Thread.Sleep(50);
			Log.Write(_connection, "test3", typeof(LogTest), string.Empty, Config.LogLevelEnum.OptionMessage);

			Log log = Log.ReadLatest(_connection);

			Assert.AreEqual("test3", log.Message);
		}
	}
}
