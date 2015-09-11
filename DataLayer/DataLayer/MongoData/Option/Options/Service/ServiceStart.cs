namespace DataLayer.MongoData.Option.Options.Service
{
	public class ServiceStart : OptionBase
	{
		public string Ip { get; set; }
		public string ServiceName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<ServiceStart>(connection);
			}
			else
			{
				Delete<ServiceStart>(connection);
			}
		}
	}
}
