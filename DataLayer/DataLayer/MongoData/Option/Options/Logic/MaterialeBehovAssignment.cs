namespace DataLayer.MongoData.Option.Options.Logic
{
	public class MaterialeBehovAssignment : OptionBase
	{
		public string urlLoginName { get; set; }
		public int updateProgressFrequency { get; set; }

		public static MaterialeBehovAssignment Create(MongoConnection connection, string name, Schedule schedule, string urlLoginName)
		{
			MaterialeBehovAssignment materialeBehovAssignment = new MaterialeBehovAssignment
			{
				urlLoginName = urlLoginName,
			};

			Create(connection, materialeBehovAssignment, name, schedule);

			return materialeBehovAssignment;
		}
	}
}
