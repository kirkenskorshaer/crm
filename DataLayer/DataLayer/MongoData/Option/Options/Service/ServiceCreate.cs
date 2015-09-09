namespace DataLayer.MongoData.Option.Options.Service
{
	public class ServiceCreate : OptionBase
	{
		public string Ip { get; set; }
		public string ServiceName { get; set; }
		public string Path { get; set; }

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
