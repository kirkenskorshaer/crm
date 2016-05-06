using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Input
{
	public class StubPusher
	{
		private static StubPusher stubWriter;
		private MongoConnection _mongoConnection;
		private static readonly object pushLock = new object();
		private int sequentilFails = 0;
		private int maxSequentilFails = 100;

		private StubPusher()
		{ }

		private StubPusher(MongoConnection mongoConnection)
		{
			_mongoConnection = mongoConnection;
		}

		public static StubPusher GetInstance(MongoConnection mongoConnection)
		{
			lock (pushLock)
			{
				if (stubWriter == null)
				{
					stubWriter = new StubPusher(mongoConnection);
				}
			}

			return stubWriter;
		}

		public void Push(Stub stub)
		{
			try
			{
				lock (pushLock)
				{
					stub.Push(_mongoConnection);
					sequentilFails = 0;
				}
			}
			catch (Exception)
			{
				sequentilFails++;
				if (sequentilFails >= maxSequentilFails)
				{
					return;
				}
				Thread.Sleep(100);
				Push(stub);
			}
		}
	}
}
