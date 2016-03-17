using DataLayer;
using NUnit.Framework;
using System.Data.SqlClient;

namespace DataLayerTest
{
	[TestFixture]
	public class TestBase
	{
		protected MongoConnection _mongoConnection;
		protected SqlConnection _sqlConnection;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_mongoConnection = MongoConnection.GetConnection("test");
			_mongoConnection.CleanDatabase();

			_sqlConnection = SqlConnectionHolder.GetConnection(_mongoConnection, "sql");
		}
	}
}
