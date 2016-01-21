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
			return Convert(dynamicsCrmConnection, fromAccount, toAccount);
		}

		public static SystemInterfaceAccount Convert(DynamicsCrmConnection dynamicsCrmConnection, DatabaseAccount fromAccount, SystemInterfaceAccount toAccount)
		{
			SystemInterfaceAccount systemInterfaceAccount = new SystemInterfaceAccount(dynamicsCrmConnection)
			{
				name = fromAccount.name,
			};

			return systemInterfaceAccount;
		}
    }
}
