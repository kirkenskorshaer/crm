namespace DataLayer.MongoData.Option.Options.Logic
{
	public class Test : OptionBase
	{
		public int SleepForSeconds { get; set; }
		public bool ThrowException { get; set; }

		public static Test Create(MongoConnection mongoConnection, string name, Schedule schedule, int sleepForSeconds, bool throwException)
		{
			Test test = new Test()
			{
				SleepForSeconds = sleepForSeconds,
				ThrowException = throwException,
			};

			Create(mongoConnection, test, name, schedule);

			return test;
		}
	}
}
