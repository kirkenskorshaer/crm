using DataLayer.MongoData.Option.Options.Data.Utilities;
using NUnit.Framework;

namespace DataLayerTest.MongoDataTest.Option.Options.Data.Utilities
{
	[TestFixture]
	public class MaintainAllTablesTest : TestBase
	{
		[Test]
		public void InsertAndDelete()
		{
			MaintainAllTables maintainAllTables = MaintainAllTables.Create(_mongoConnection, "test", MongoTestUtilities.CreateOneTimeSimpleSchedule());

			maintainAllTables.Delete(_mongoConnection);
		}
	}
}
