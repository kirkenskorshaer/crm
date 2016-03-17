using DataLayer.SqlData.Byarbejde;
using NUnit.Framework;

namespace DataLayerTest.SqlDataTest.ByarbejdeTest
{
	[TestFixture]
	public class ByarbejdeTest: TestBase
	{
		[Test]
		public void MaintainTable()
		{
			Byarbejde.MaintainTable(_sqlConnection);
		}
    }
}
