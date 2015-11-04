namespace DataLayer.MongoData.Option.Options.Data.Utilities
{
	public class MaintainAllTables : OptionBase
	{
		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<MaintainAllTables>(connection);
			}
			else
			{
				Delete<MaintainAllTables>(connection);
			}
		}
	}
}