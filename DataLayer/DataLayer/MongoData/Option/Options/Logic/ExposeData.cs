namespace DataLayer.MongoData.Option.Options.Logic
{
	public class ExposeData : OptionBase
	{
		public string urlLoginName { get; set; }
		public string fetchXmlPath { get; set; }
		public string exposePath { get; set; }
		public string exposeName { get; set; }

		public static ExposeData Create(MongoConnection mongoConnection, string urlLoginName, string name, Schedule schedule, Schedule importFromStubSchedule)
		{
			ExposeData exposeIndsamlingssteder = new ExposeData()
			{
				urlLoginName = urlLoginName,
			};

			Create(mongoConnection, exposeIndsamlingssteder, name, schedule);

			return exposeIndsamlingssteder;
		}
	}
}
