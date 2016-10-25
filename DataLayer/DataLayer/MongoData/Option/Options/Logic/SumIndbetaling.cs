namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SumIndbetaling : OptionBase
	{
		public string urlLoginName { get; set; }

		public static SumIndbetaling Create(MongoConnection mongoConnection, string urlLoginName, string name, Schedule schedule)
		{
			SumIndbetaling sumIndbetaling = new SumIndbetaling()
			{
				urlLoginName = urlLoginName,
			};

			Create(mongoConnection, sumIndbetaling, name, schedule);

			return sumIndbetaling;
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SumIndbetaling>(connection);
			}
			else
			{
				Delete<SumIndbetaling>(connection);
			}
		}
	}
}
