namespace DataLayer.MongoData.Option.Options.Logic
{
	public class MaintainProgress : OptionBase
	{
		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<MaintainProgress>(connection);
			}
			else
			{
				Delete<MaintainProgress>(connection);
			}
		}
	}
}
