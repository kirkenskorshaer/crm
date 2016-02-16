using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using SystemInterface.Dynamics.Crm;
using System.Collections.Generic;

namespace Administration.Conversion
{
	public static class Account
	{
		public static SystemInterfaceAccount Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseAccount fromAccount)
		{
			SystemInterfaceAccount toAccount = new SystemInterfaceAccount(dynamicsCrmConnection);
			Convert(dynamicsCrmConnection, fromAccount, toAccount);

			return toAccount;
		}

		public static void Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseAccount fromAccount, SystemInterfaceAccount toAccount)
		{
			List<string> exclusionList = new List<string>() { "Id" };
			List<string> keys = Utilities.ReflectionHelper.GetFieldsAndProperties(typeof(DatabaseAccount), exclusionList);

			foreach (string key in keys)
			{
				object value = Utilities.ReflectionHelper.GetValue(fromAccount, key);
				Utilities.ReflectionHelper.SetValue(toAccount, key, value);
			}
		}
	}
}
