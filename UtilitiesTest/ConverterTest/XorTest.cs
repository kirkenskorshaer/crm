using NUnit.Framework;
using System;
using Utilities.Converter;

namespace UtilitiesTest.ConverterTest
{
	[TestFixture]
	public class XorTest
	{
		[Test]
		public void XorHandlesNull()
		{
			string aString = "aString";
			string nullString = null;

			string xorWithNull = Xor.XorString(aString, nullString);

			Assert.AreEqual(aString, xorWithNull);
		}

		[Test]
		public void XorOrderIsIrellevant()
		{
			string shortString = "shortString";
			string aLongerString = "a longer string";

			string xorShortLong = Xor.XorString(shortString, aLongerString);
			string xorLongShort = Xor.XorString(aLongerString, shortString);

			Console.Out.WriteLine(xorShortLong);

			Assert.AreEqual(xorShortLong, xorLongShort);
		}

		[Test]
		public void XorGuidOrderIsIrellevant()
		{
			Guid guidA = Guid.NewGuid();
			Guid guidB = Guid.NewGuid();

			Guid xorAB = Xor.XorGuid(guidA, guidB);
			Guid xorBA = Xor.XorGuid(guidB, guidA);

			Console.Out.WriteLine(xorAB);

			Assert.AreEqual(xorAB, xorBA);
		}

		[Test]
		public void XorWitEmptyIsSameAsOriginal()
		{
			string emptyString = string.Empty;
			string nonEmptyString = "non empty string";

			string xorWithEmpty = Xor.XorString(emptyString, nonEmptyString);

			Console.Out.WriteLine(xorWithEmpty);

			Assert.AreEqual(nonEmptyString, xorWithEmpty);
		}
	}
}
