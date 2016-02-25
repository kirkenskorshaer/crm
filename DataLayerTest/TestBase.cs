using DataLayer;
using NUnit.Framework;

namespace DataLayerTest
{
	[TestFixture]
	public class TestBase
	{
		protected MongoConnection _mongoConnection;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
			_mongoConnection.CleanDatabase();
		}
	}
}
