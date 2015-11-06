using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;
using System;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class ProgressTest
	{
		private MongoConnection _connection;
		private string testTargetName = "testTarget";

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();
		}

		[Test]
		public void ReadNextReturnsNullIfNoProgressExists()
		{
			Progress progress = Progress.ReadNext(_connection, testTargetName);

			Assert.IsNull(progress);
		}

		[Test]
		public void ReadNextReturnsNullIfNoProgressExistsOnTheGivenTarget()
		{
			Progress createdProgress = CreateProgress(DateTime.Now);

			Progress progress = Progress.ReadNext(_connection, "testTargetWrongName");

			Assert.IsNull(progress);
		}

		[Test]
		public void ReadNextReturnsNextProgress()
		{
			Progress createdProgress1 = CreateProgress(new DateTime(2000, 1, 2));
			Progress createdProgress2 = CreateProgress(new DateTime(2000, 1, 1));
			Progress createdProgress3 = CreateProgress(new DateTime(2000, 1, 3));

			Progress progress = Progress.ReadNext(_connection, testTargetName);

			Assert.AreEqual(createdProgress2.TargetId, progress.TargetId);
		}

		[Test]
		public void UpdateLastProgressDateToNowUpdatesProgress()
		{
			Progress createdProgress = CreateProgress(new DateTime(2000, 1, 2));
			createdProgress.UpdateLastProgressDateToNow(_connection);

			Progress progress = Progress.ReadNext(_connection, testTargetName);

			Assert.AreEqual(DateTime.Now.Year, progress.LastProgressDate.Year);
		}

		private Progress CreateProgress(DateTime progressDate)
		{
			Progress createdProgress = new Progress()
			{
				LastProgressDate = progressDate,
				TargetId = Guid.NewGuid(),
				TargetName = testTargetName
			};

			createdProgress.Insert(_connection);

			return createdProgress;
		}
	}
}
