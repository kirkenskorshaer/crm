using System;
using System.Diagnostics;
using Administration;
using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Options;
using NUnit.Framework;
using DataLayer.MongoData.Option.Options.Logic;
using System.Threading;

namespace AdministrationTest
{
	[TestFixture]
	public class HeartTest : TestBase
	{
		[Test]
		public void HeartBeatPrefersEmail()
		{
			Email.Create(Connection, "testName", new Schedule(), "testTo", "testMessageBody");

			Heart heart = new Heart();

			try
			{
				heart.HeartBeat();
			}
			catch (FormatException exception)
			{
				Assert.AreEqual("The specified string is not in the form required for an e-mail address.", exception.Message);
				return;
			}

			Assert.Fail();
		}

		[Test]
		public void HeartBeatSleepsWhenThereIsNothingToDo()
		{
			Heart heart = new Heart();

			heart.Begin();

			Stopwatch stopwatch = Stopwatch.StartNew();
			heart.HeartBeat();
			stopwatch.Stop();

			heart.End();

			Assert.Greater(stopwatch.ElapsedMilliseconds, 100);
		}

		[Test]
		[Ignore]
		public void HeartBeatWithOptions()
		{
			Heart heart = new Heart();

			heart.Begin();

			AddExposeDataOption(Schedule.ActionOnFailEnum.Disable);
			AddExposeDataOption(Schedule.ActionOnFailEnum.TryAgain);
			AddExposeDataOption(Schedule.ActionOnFailEnum.TryAgain);
			AddExposeDataOption(Schedule.ActionOnFailEnum.TryAgain);
			AddExposeDataOption(Schedule.ActionOnFailEnum.TryAgain);

			AddTestOption(Schedule.ActionOnFailEnum.TryAgain, 10, true);
			AddTestOption(Schedule.ActionOnFailEnum.TryAgain, 10, false);
			AddTestOption(Schedule.ActionOnFailEnum.TryAgain, 10, false);
			AddTestOption(Schedule.ActionOnFailEnum.TryAgain, 10, true);

			do
			{
				heart.HeartBeat();

				Thread.Sleep(TimeSpan.FromSeconds(5));
			}
			while (heart.IsAnyThreadHolderBusy() || heart.IsWorkQueued());

			heart.End();
		}

		public void AddExposeDataOption(Schedule.ActionOnFailEnum actionOnFail)
		{
			ExposeData exposeData = ExposeData.Create(Connection, "test", "test", CreateScheduleAlwaysOnDoOnce());

			exposeData.exposeName = "test.json";
			exposeData.exposePath = "heartTest";
			exposeData.fetchXmlPath = "Dynamics/Crm/FetchXml/Account/plant.xml";
			exposeData.Schedule.ActionOnFail = actionOnFail;

			exposeData.UpdateOption(Connection);
		}

		public void AddTestOption(Schedule.ActionOnFailEnum actionOnFail, int sleepForSeconds, bool throwException)
		{
			Test test = Test.Create(Connection, "test", CreateScheduleAlwaysOnDoOnce(), sleepForSeconds, throwException);

			test.Schedule.ActionOnFail = actionOnFail;

			test.UpdateOption(Connection);
		}

		[Test]
		[Ignore]
		public void RunTest()
		{
			Heart heart = new Heart();
			heart.Run();
		}
	}
}
