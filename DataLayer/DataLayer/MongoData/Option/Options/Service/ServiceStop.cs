namespace DataLayer.MongoData.Option.Options.Service
{
	public class ServiceStop : OptionBase
	{
		public string Ip { get; set; }
		public string ServiceName { get; set; }

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
