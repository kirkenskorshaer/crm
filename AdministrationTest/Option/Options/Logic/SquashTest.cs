using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DatabaseContactChange = DataLayer.SqlData.Contact.ContactChange;

namespace AdministrationTest.Option.Options.Logic
{
	[TestFixture]
	public class SquashTest
	{
		[Test]
		public void find()
		{
			Type changeType = typeof(DatabaseContactChange);

			FieldInfo[] fieldsInfo = changeType.GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach(FieldInfo fieldInfo in fieldsInfo)
			{
				Console.WriteLine(fieldInfo.Name);
			}

			PropertyInfo[] propertiesInfo = changeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo propertyInfo in propertiesInfo)
			{
				Console.WriteLine(propertyInfo.Name);
			}
		}
	}
}
