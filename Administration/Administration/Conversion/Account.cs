using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DatabaseAccount = DataLayer.SqlData.Account.Account;
using SystemInterface.Dynamics.Crm;

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
			toAccount.name = fromAccount.name;
		}
	}
}
