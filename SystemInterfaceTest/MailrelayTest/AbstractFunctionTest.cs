using NUnit.Framework;
using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay;
using SystemInterface.Mailrelay.Function.CustomFields;
using SystemInterface.Mailrelay.Function.Groups;
using SystemInterface.Mailrelay.Function.LogFunctions;
using SystemInterface.Mailrelay.Function.Statistics;
using SystemInterface.Mailrelay.Function.Subscribers;

namespace SystemInterfaceTest.MailrelayTest
{
	[TestFixture]
	public class AbstractFunctionTest : TestBase
	{
		[Test]
		public void ToGetCreatesUrlEncodedRepresentationOfValues()
		{
			string expectedUrlEncoded = $"&function=getStats&id=1&startDate=2000-01-01+00%3a00%3a00&endDate=2001-01-01+00%3a00%3a00";

			getStats function = new getStats()
			{
				startDate = new DateTime(2000, 1, 1),
				endDate = new DateTime(2001, 1, 1),
				id = 1,
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithEnumCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=getGroups&sortOrder=ASC";

			getGroups function = new getGroups()
			{
				sortOrder = AbstractFunction.sortOrderEnum.ASC,
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithToGetAttributeCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=addCustomField&name=test&fieldType=3&position=1&default=defaultValue&enable=1";

			addCustomField function = new addCustomField()
			{
				fieldType = AbstractCustomField.CustomFieldTypeEnum.Textarea,
				position = 1,
				name = "test",
				defaultField = "defaultValue",
				enable = true,
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithIntArrayCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=getSubscribers&groups%5b0%5d=1&groups%5b1%5d=4";

			getSubscribers function = new getSubscribers()
			{
				groups = new List<int>() { 1, 4 },
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithStringArrayCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=assignSubscribersToGroups&groups%5b0%5d=1&groups%5b1%5d=4&subscribers%5b0%5d=test";

			assignSubscribersToGroups function = new assignSubscribersToGroups()
			{
				groups = new List<int>() { 1, 4 },
				subscribers = new List<string>() { "test" },
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithSingleElementArrayCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=getSubscribers&groups%5b0%5d=1";

			getSubscribers function = new getSubscribers()
			{
				groups = new List<int>() { 1 },
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithEmptyArrayCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=getSubscribers";

			getSubscribers function = new getSubscribers()
			{
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithDictionaryCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=addSubscriber&customFields%5bf_1%5d=test+1&customFields%5bf_2%5d=test+2";

			addSubscriber function = new addSubscriber()
			{
				customFields = new Dictionary<string, string>()
				{
					{ "f_1", "test 1" },
					{ "f_2", "test 2" },
				},
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithSingleElementDictionaryCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=addSubscriber&customFields%5bf_1%5d=test+1";

			addSubscriber function = new addSubscriber()
			{
				customFields = new Dictionary<string, string>()
				{
					{ "f_1", "test 1" },
				},
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithEmptyDictionaryCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=addSubscriber";

			addSubscriber function = new addSubscriber()
			{
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		[Test]
		public void functionWithShortDateCanBeUrlEncoded()
		{
			string expectedUrlEncoded = $"&function=getDeliveryErrors&date=2000-01-01";

			getDeliveryErrors function = new getDeliveryErrors()
			{
				date = new DateTime(2000, 1, 1),
			};

			AssertToGet(function, expectedUrlEncoded);
		}

		private void AssertToGet(AbstractFunction function, string expectedEncoded)
		{
			string apiKey = "testApiKey";
			string expectedUrlEncoded = $"{expectedEncoded}&apiKey={apiKey}";
			function.apiKey = apiKey;

			string output = function.ToGet();

			Assert.AreEqual(expectedUrlEncoded, output);
		}
	}
}
