using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class UpdateMailrelayFromContact : OptionBase
	{
		public string urlLoginName { get; set; }
		public int pageSize { get; set; }
		public Guid? contactId { get; set; }

		public static UpdateMailrelayFromContact Create(MongoConnection mongoConnection, string urlLoginName, string name, Schedule schedule)
		{
			UpdateMailrelayFromContact updateMailrelayCheck = new UpdateMailrelayFromContact()
			{
				urlLoginName = urlLoginName,
			};

			Create(mongoConnection, updateMailrelayCheck, name, schedule);

			return updateMailrelayCheck;
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<UpdateMailrelayFromContact>(connection);
			}
			else
			{
				Delete<UpdateMailrelayFromContact>(connection);
			}
		}
	}
}
