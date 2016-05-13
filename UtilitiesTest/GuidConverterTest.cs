using NUnit.Framework;
using System;

namespace UtilitiesTest
{
	[TestFixture]
	public class GuidConverterTest
	{
		[TestCase(0, 0, 0, 0, "00000000-0000-0000-0000-000000000000")]
		[TestCase(2147483647, 0, 0, 0, "7fffffff-0000-0000-0000-000000000000")]
		[TestCase(0, 2147483647, 0, 0, "00000000-ffff-7fff-0000-000000000000")]
		[TestCase(0, 0, 2147483647, 0, "00000000-0000-0000-ffff-ff7f00000000")]
		[TestCase(0, 0, 0, 2147483647, "00000000-0000-0000-0000-0000ffffff7f")]
		[TestCase(2147483647, 2147483647, 2147483647, 2147483647, "7fffffff-ffff-7fff-ffff-ff7fffffff7f")]
		public void IntConversion(int source1, int source2, int source3, int source4, string result)
		{
			Guid resultGuid = Guid.Parse(result);

			Guid convertedGuid = Utilities.Converter.GuidConverter.Convert(source1, source2, source3, source4);

			Assert.AreEqual(resultGuid, convertedGuid);
		}

		[Test]
		public void StringConversion()
		{
			string input = "lots of random characters qwertyuiopå¨^asdfghjklæø'zxcvbnm,.-∠°²³∯@私";

			Guid resultGuid = Utilities.Converter.GuidConverter.Convert(input);

			Assert.AreNotEqual(Guid.Empty, resultGuid);
		}
	}
}
