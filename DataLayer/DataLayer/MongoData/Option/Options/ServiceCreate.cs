namespace DataLayer.MongoData.Option.Options
{
	public class ServiceCreate : OptionBase
	{
		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ServiceCreate>(connection);
			}
			else
			{
				Delete<ServiceCreate>(connection);
			}
		}
	}
}
