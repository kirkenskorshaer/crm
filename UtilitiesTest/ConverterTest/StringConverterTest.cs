using NUnit.Framework;
using System;
using System.Collections.Generic;
using Utilities.Converter;

namespace UtilitiesTest.ConverterTest
{
	[TestFixture]
	public class StringConverterTest
	{
		[Test]
		public void StringCanBeSerializedAndDeserialized()
		{
			string before = $"string id is = {Guid.NewGuid()} and special characters ½§!\"#¤%&/()=?`@£$€{{[]}}|æøå¨'^*~-.,_:;<>\\";

			string serialized = StringConverter.SerializeToString(before);

			string deserialized = (string)StringConverter.DeserializeFromString(serialized);

			Assert.AreEqual(before, deserialized);
		}

		[Test]
		public void GuidCanBeSerializedAndDeserialized()
		{
			Guid before = Guid.NewGuid();

			string serialized = StringConverter.SerializeToString(before);

			Guid deserialized = (Guid)StringConverter.DeserializeFromString(serialized);

			Assert.AreEqual(before, deserialized);
		}

		[Test]
		public void IntCanBeSerializedAndDeserialized()
		{
			int before = int.MaxValue;

			string serialized = StringConverter.SerializeToString(before);

			int deserialized = (int)StringConverter.DeserializeFromString(serialized);

			Assert.AreEqual(before, deserialized);
		}

		[Test]
		public void ComplexObjectCanBeSerializedAndDeserialized()
		{
			Dictionary<Guid, string> before = new Dictionary<Guid, string>()
			{
				{ Guid.NewGuid(), "fjhadfasrfjhgsdfherw" },
				{ Guid.NewGuid(), "2341hrf041140" },
				{ Guid.NewGuid(), "æø',:F ef0" },
			};

			string serialized = StringConverter.SerializeToString(before);

			Dictionary<Guid, string> deserialized = (Dictionary<Guid, string>)StringConverter.DeserializeFromString(serialized);

			Assert.AreEqual(before, deserialized);
		}
	}
}
