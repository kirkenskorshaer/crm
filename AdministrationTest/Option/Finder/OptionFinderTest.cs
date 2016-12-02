using Administration.Option;
using Administration.Option.Finder;
using Administration.Option.Options;
using Administration.Option.Options.Logic;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DatabaseExposeData = DataLayer.MongoData.Option.Options.Logic.ExposeData;

namespace AdministrationTest.Option.Finder
{
	[TestFixture]
	public class OptionFinderTest : TestBase
	{
		[Test]
		public void FindFindsOption()
		{
			OptionFinder finder = new OptionFinder(Connection);

			DatabaseExposeData.Create(Connection, "test", "test", CreateScheduleAlwaysOnDoOnce(), CreateScheduleAlwaysOnDoOnce());

			List<OptionBase> options = finder.Find();

			Assert.True(options.Count == 2);
			Assert.True(options.Any(option => option.GetType() == typeof(ExposeData)));
			Assert.True(options.Any(option => option.GetType() == typeof(Sleep)));
		}
	}
}
