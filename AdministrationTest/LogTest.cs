using Administration;
using NUnit.Framework;
using System.Threading;
using DataLayerLog = DataLayer.MongoData.Log;

namespace AdministrationTest
{
	[TestFixture]
	public class LogTest : TestBase
	{
		[Test]
		public void LogWritesNewMessages()
		{
			string testMessage = "test message 1";
			Log.Write(Connection, testMessage, DataLayer.MongoData.Config.LogLevelEnum.HeartError);

			DataLayerLog logRead = DataLayerLog.ReadLatest(Connection);

			Assert.AreEqual(testMessage, logRead.Message);
		}

		[Test]
		public void LogDoesNotWriteDuplicateMessages()
		{
			string testMessage = "test message 1";
			Log.Write(Connection, testMessage, DataLayer.MongoData.Config.LogLevelEnum.HeartError);
			DataLayerLog logRead1 = DataLayerLog.ReadLatest(Connection);

			Thread.Sleep(100);
			Log.Write(Connection, testMessage, DataLayer.MongoData.Config.LogLevelEnum.HeartError);
			DataLayerLog logRead2 = DataLayerLog.ReadLatest(Connection);

			Assert.AreEqual(logRead1._id, logRead2._id);
		}

		[Test]
		public void LogWritesDuplicateMessagesIfDelayIsLow()
		{
			string testMessage = "test message 1";
			Log.MaxSecondsToDiscardIdenticalLogMessages = 0;

			Log.Write(Connection, testMessage, DataLayer.MongoData.Config.LogLevelEnum.HeartError);
			DataLayerLog logRead1 = DataLayerLog.ReadLatest(Connection);

			Thread.Sleep(100);
			Log.Write(Connection, testMessage, DataLayer.MongoData.Config.LogLevelEnum.HeartError);
			DataLayerLog logRead2 = DataLayerLog.ReadLatest(Connection);

			Assert.AreNotEqual(logRead1._id, logRead2._id);
		}
	}
}
