namespace DataLayer.MongoData.Option.Options.Logic
{
	public class CreateImportFromStub : OptionBase
	{
		public string urlLoginName { get; set; }
		public Schedule ImportFromStubSchedule { get; set; }

		public static CreateImportFromStub Create(MongoConnection mongoConnection, string urlLoginName, string name, Schedule schedule, Schedule importFromStubSchedule)
		{
			CreateImportFromStub createImportFromStub = new CreateImportFromStub()
			{
				urlLoginName = urlLoginName,
				ImportFromStubSchedule = importFromStubSchedule,
			};

			Create(mongoConnection, createImportFromStub, name, schedule);

			return createImportFromStub;
		}
	}
}