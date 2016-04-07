using System.Collections.Generic;
using SystemInterface.Dynamics.Crm;
using SystemInterfaceByarbejde = SystemInterface.Dynamics.Crm.Byarbejde;
using DatabaseByarbejde = DataLayer.SqlData.Byarbejde.Byarbejde;

namespace Administration.Conversion
{
	public static class Byarbejde
	{
		public static SystemInterfaceByarbejde Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseByarbejde fromByarbejde)
		{
			SystemInterfaceByarbejde toByarbejde = new SystemInterfaceByarbejde(dynamicsCrmConnection);
			Convert(fromByarbejde, toByarbejde);

			return toByarbejde;
		}

		public static void Convert(DatabaseByarbejde fromByarbejde, SystemInterfaceByarbejde toByarbejde)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseByarbejde), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromByarbejde, key);
				Utilities.ReflectionHelper.SetValue(toByarbejde, key, value);
			}
		}

		public static void Convert(SystemInterfaceByarbejde fromByarbejde, DatabaseByarbejde toByarbejde)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(SystemInterfaceByarbejde), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromByarbejde, key);
				Utilities.ReflectionHelper.SetValue(toByarbejde, key, value);
			}
		}
	}
}
