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

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ExposeData>(connection);
			}
			else
			{
				Delete<ExposeData>(connection);
			}
		}
	}
}
