namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SendSms : OptionBase
	{
		public string urlLoginName { get; set; }

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SendSms>(connection);
			}
			else
			{
				Delete<SendSms>(connection);
			}
		}
	}
}
