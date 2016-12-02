using DataLayer.MongoData.Option;
using DataLayer.MongoData.Option.Finder;
using DataLayer.MongoData.Option.Options;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DataLayerTest.MongoDataTest.Option.Finder
{
	[TestFixture]
	public class OptionFinderTest : TestBase
	{
		[Test]
		public void FindFindsOption()
		{
			OptionFinder finder = new OptionFinder(_mongoConnection);

			Email.Create(_mongoConnection, "test", new Schedule(), "", "");

			List<OptionBase> options = finder.Find();

			Assert.True(options.Single().GetType() == typeof(Email));
		}
	}
}
