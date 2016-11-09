using NUnit.Framework;
using System.Collections.Generic;
using Utilities;

namespace UtilitiesTest
{
	[TestFixture]
	public class TextMergerTest
	{
		[Test]
		public void TextMergerInsertsValues()
		{
			string originalText = "test1 {!test1;default}";

			TextMerger textMerger = new TextMerger(originalText);

			string merged = textMerger.GetMerged(new Dictionary<string, object>() { { "test1", "result1" } }, "yyyy-MM-dd");

			string expected = "test1 result1";

			Assert.AreEqual(expected, merged);
		}

		[Test]
		public void TextMergerCanMergeMultipleTexts()
		{
			string originalText = "test1 {!test1;default}";

			TextMerger textMerger = new TextMerger(originalText);

			string merged1 = textMerger.GetMerged(new Dictionary<string, object>() { { "test1", "result1" } }, "yyyy-MM-dd");
			string merged2 = textMerger.GetMerged(new Dictionary<string, object>() { { "test1", "result2" } }, "yyyy-MM-dd");

			string expected1 = "test1 result1";
			string expected2 = "test1 result2";

			Assert.AreEqual(expected1, merged1);
			Assert.AreEqual(expected2, merged2);
		}
	}
}
