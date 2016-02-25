namespace DataLayer.MongoData.Option.Options.Data.Utilities
{
	public class MaintainAllTables : OptionBase
	{
		public static MaintainAllTables Create(MongoConnection connection, string name, Schedule schedule)
		{
			MaintainAllTables maintainAllTables = new MaintainAllTables
			{
			};

			Create(connection, maintainAllTables, name, schedule);

			return maintainAllTables;
		}

		public void Delete(MongoConnection connection)
		{
			Delete<MaintainAllTables>(connection);
		}

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