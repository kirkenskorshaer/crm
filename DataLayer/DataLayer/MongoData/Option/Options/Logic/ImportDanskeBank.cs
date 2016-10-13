namespace DataLayer.MongoData.Option.Options.Logic
{
	public class ImportDanskeBank : OptionBase
	{
		public string urlLoginName { get; set; }
		public string importFolder { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ImportDanskeBank>(connection);
			}
			else
			{
				Delete<ImportDanskeBank>(connection);
			}
		}
	}
}
