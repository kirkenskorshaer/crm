using System.Threading;
using DataLayer;

namespace Administration.Option.Options
{
	public class Sleep : OptionBase
	{
		internal static int SleepTime = 100;

		public Sleep(MongoConnection connection) : base(connection, null)
		{
		}

		public override void Execute()
		{
			Thread.Sleep(SleepTime);
		}
	}
}
