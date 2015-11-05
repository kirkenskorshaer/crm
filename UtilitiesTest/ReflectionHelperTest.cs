using NUnit.Framework;
using System.Collections.Generic;
using Utilities;

namespace UtilitiesTest
{
	[TestFixture]
	public class ReflectionHelperTest
	{
		public class TestClass
		{
			public string Field1;
			public string Field2;
			public string Property1 { get; set; }
			public string Property2 { get; set; }
		}

		[Test]
		public void GetFieldsAndPropertiesReturnsAllFieldsAndPropertiesNotInExclusionList()
		{
			List<string> exclusionList = new List<string>() { "Field1", "Property2" };

			List<string> fieldsAndProperties = ReflectionHelper.GetFieldsAndProperties(typeof(TestClass), exclusionList);

			List<string> expectedList = new List<string>() { "Field2", "Property1" };
			Assert.AreEqual(expectedList, fieldsAndProperties);
		}

		[Test]
		public void GetValueReturnsFieldValue()
		{
			string fieldValue = "testField";

			TestClass testObject = new TestClass();
			testObject.Field1 = fieldValue;

			string returnedValue = (string)ReflectionHelper.GetValue(testObject, "Field1");

			Assert.AreEqual(fieldValue, returnedValue);
		}

		[Test]
		public void GetValueReturnsPropertyValue()
		{
			string propertyValue = "testProperty";

			TestClass testObject = new TestClass();
			testObject.Property2 = propertyValue;

			string returnedValue = (string)ReflectionHelper.GetValue(testObject, "Property2");

			Assert.AreEqual(propertyValue, returnedValue);
		}

		[Test]
		public void SetValueSetsFieldValue()
		{
			string fieldValue = "testField";

			TestClass testObject = new TestClass();

			ReflectionHelper.SetValue(testObject, "Field1", fieldValue);

			string returnedValue = testObject.Field1;
			Assert.AreEqual(fieldValue, returnedValue);
		}

		[Test]
		public void SetValueSetsPropertyValue()
		{
			string propertyValue = "testProperty";

			TestClass testObject = new TestClass();

			ReflectionHelper.SetValue(testObject, "Property2", propertyValue);

			string returnedValue = testObject.Property2;
			Assert.AreEqual(propertyValue, returnedValue);
		}
	}
}
