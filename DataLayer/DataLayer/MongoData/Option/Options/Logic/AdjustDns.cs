namespace DataLayer.MongoData.Option.Options.Logic
{
	public class AdjustDns : OptionBase
	{
		public string dnsIp { get; set; }
		public string adapterName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<AdjustDns>(connection);
			}
			else
			{
				Delete<AdjustDns>(connection);
			}
		}
	}
}
