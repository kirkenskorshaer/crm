using System.Threading;
using DataLayer;

namespace Administration.Option.Options
{
	public class Sleep : OptionBase
	{
		internal static int SleepTime = 1000;

		public Sleep(MongoConnection connection) : base(connection, null)
		{
		}

		protected override void ExecuteOption(OptionReport report)
		{
			Thread.Sleep(SleepTime);
			report.Success = true;
			return;
		}
	}
}
