using DataLayer.MongoData.Input;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace DataLayerTest.MongoDataTest.InputTest
{
	[TestFixture]
	public class StubTest : TestBase
	{
		[Test]
		public void StubCanBeInserted()
		{
			Stub stub = PushStub();

			Stub stubRead = Stub.ReadFirst(_mongoConnection);

			Assert.AreEqual(stub.PostTime.ToString(), stubRead.PostTime.ToString());
		}

		[Test]
		public void StubCanBeDeleted()
		{
			Stub stub = PushStub();
			stub.Delete(_mongoConnection);

			Stub stubRead = Stub.ReadFirst(_mongoConnection);

			Assert.IsNull(stubRead);
		}

		private int completedWorkers;
		private int failedWorkers;
		[Test]
		[Ignore("")]
		public void StubCanHandleLotsOfPreasure()
		{
			DateTime beginTime = DateTime.Now;
			int numberOfWorkers = 100;
			int numberOfStubsPerWorker = 100;
			completedWorkers = 0;
			failedWorkers = 0;
			
			/*
			for (int i = 0; i < numberOfWorkers * numberOfStubsPerWorker; i++)
			{
				PushStub();
			}
			*/

			List<BackgroundWorker> workers = new List<BackgroundWorker>();

			for (int currentWorkerIndex = 0; currentWorkerIndex < numberOfWorkers; currentWorkerIndex++)
			{
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += PushManyStubs;

				workers.Add(worker);
			}

			workers.ForEach(worker => worker.RunWorkerAsync(numberOfStubsPerWorker));

			while ((completedWorkers + failedWorkers) < numberOfWorkers)
			{
				Thread.Sleep(100);
			}

			DateTime endTime = DateTime.Now;

			TimeSpan runTime = endTime - beginTime;
			Assert.AreEqual(0, failedWorkers);
			Assert.AreEqual(numberOfWorkers * numberOfStubsPerWorker, Stub.Count(_mongoConnection));
			Console.Out.WriteLine($"runtime = {runTime}");
		}

		private void PushManyStubs(object sender, DoWorkEventArgs e)
		{
			int numberOfStubs = (int)e.Argument;

			try
			{
				for (int currentStubIndex = 0; currentStubIndex < numberOfStubs; currentStubIndex++)
				{
					PushStub();
				}
				ReportCompletion(true);
			}
			catch (Exception exception)
			{
				//Log.Write(_mongoConnection, exception.Message, exception.StackTrace, Config.LogLevelEnum.OptionMessage);
				ReportCompletion(false);
			}
		}

		private object lockObject = new object();
		private void ReportCompletion(bool isSuccess)
		{
			lock (lockObject)
			{
				if (isSuccess)
				{
					completedWorkers++;
				}
				else
				{
					failedWorkers++;
				}
			}
		}

		private Stub PushStub()
		{
			Stub stub = new Stub()
			{
				Contents = new List<StubElement>()
				{
					new StubElement() { Key = "firstname", Value = $"firstname {Guid.NewGuid()}" }
					,new StubElement() { Key = "lastname", Value = $"lastname {Guid.NewGuid()}" }
				},
				PostTime = DateTime.Now,
			};

			StubPusher.GetInstance(_mongoConnection).Push(stub);

			return stub;
		}
	}
}
