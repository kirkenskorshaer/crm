namespace DataLayer.MongoData.Option.Options.Data.ChangeProvider
{
	public class ChangeProviderCreate : OptionBase
	{
		public string ProviderName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ChangeProviderCreate>(connection);
			}
			else
			{
				Delete<ChangeProviderCreate>(connection);
			}
		}
	}
}
