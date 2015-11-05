namespace DataLayer.MongoData.Option.Options.Logic
{
	public class Squash : OptionBase
	{
		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<Squash>(connection);
			}
			else
			{
				Delete<Squash>(connection);
			}
		}
	}
}
