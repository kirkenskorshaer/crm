namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SetMissingCampaignLeadtarget : OptionBase
	{
		public string urlLoginName { get; set; }
		public string baseUrl { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SetMissingCampaignLeadtarget>(connection);
			}
			else
			{
				Delete<SetMissingCampaignLeadtarget>(connection);
			}
		}
	}
}
