namespace DataLayer.MongoData.Option.Options.Data.ChangeProvider
{
	public class ChangeProviderReadAll : OptionBase
	{
		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ChangeProviderReadAll>(connection);
			}
			else
			{
				Delete<ChangeProviderReadAll>(connection);
			}
		}
	}
}