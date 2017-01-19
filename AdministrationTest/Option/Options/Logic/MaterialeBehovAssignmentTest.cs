using NUnit.Framework;
using DatabaseMaterialeBehovAssignment = DataLayer.MongoData.Option.Options.Logic.MaterialeBehovAssignment;
using Administration.Option.Options.Logic;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class MaterialeBehovAssignmentTest : TestBase
	{
		[Test]
		[Ignore]
		public void Test()
		{
			DatabaseMaterialeBehovAssignment databaseMaterialeBehovAssignment = new DatabaseMaterialeBehovAssignment()
			{
				urlLoginName = "test",
				updateProgressFrequency = 1,
				Schedule = CreateScheduleAlwaysOnDoOnce(),
			};

			MaterialeBehovAssignment materialeBehovAssignment = new MaterialeBehovAssignment(Connection, databaseMaterialeBehovAssignment);

			Administration.Option.Options.OptionReport report = new Administration.Option.Options.OptionReport("test");
			materialeBehovAssignment.ExecuteOption(report);
			bool isSuccess = report.Success;

			Assert.IsTrue(isSuccess);
		}
	}
}
