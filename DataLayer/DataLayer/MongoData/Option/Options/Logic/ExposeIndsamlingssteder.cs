namespace DataLayer.MongoData.Option.Options.Logic
{
	public class ExposeIndsamlingssteder : OptionBase
	{
		public string urlLoginName { get; set; }

		public static ExposeIndsamlingssteder Create(MongoConnection mongoConnection, string urlLoginName, string name, Schedule schedule, Schedule importFromStubSchedule)
		{
			ExposeIndsamlingssteder exposeIndsamlingssteder = new ExposeIndsamlingssteder()
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
				Update<ExposeIndsamlingssteder>(connection);
			}
			else
			{
				Delete<ExposeIndsamlingssteder>(connection);
			}
		}
	}
}
