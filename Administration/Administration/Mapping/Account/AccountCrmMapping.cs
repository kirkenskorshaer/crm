using DataBaseAccount = DataLayer.SqlData.Account.Account;
using DataBaseExternalAccount = DataLayer.SqlData.Account.ExternalAccount;
using DataBaseAccountChange = DataLayer.SqlData.Account.AccountChange;
using SystemInterfaceAccount = SystemInterface.Dynamics.Crm.Account;
using DataLayer;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Administration.Mapping.Account
{
	public static class AccountCrmMapping
	{
		public static DataBaseAccount FindAccount(MongoConnection mongoConnection, SqlConnection sqlConnection, Guid externalAccountId, SystemInterfaceAccount crmAccount)
		{
			List<DataBaseAccount> accountsPreviouslyChanged = DataBaseAccountChange.GetAccounts(sqlConnection, externalAccountId);

			if (accountsPreviouslyChanged.Count == 0)
			{
				return null;
			}

			if (accountsPreviouslyChanged.Count == 1)
			{
				return accountsPreviouslyChanged[0];
			}

			Log.Write(mongoConnection, $"multible account match for {externalAccountId}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);

			return accountsPreviouslyChanged.OrderByDescending(account => account.CreatedOn).First();
		}

		public static List<DataBaseExternalAccount> FindAccounts(MongoConnection connection, SqlConnection sqlConnection, DataBaseAccount account, Guid changeProviderId)
		{
			Guid accountId = account.Id;

			List<DataBaseExternalAccount> externalAccounts = DataBaseExternalAccount.ReadFromChangeProviderAndAccount(sqlConnection, changeProviderId, accountId);

			if (externalAccounts.Count == 0)
			{
				Log.Write(connection, $"no external account found for changeProvider {changeProviderId} and account {accountId}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}

			if (externalAccounts.Count > 1)
			{
				Log.Write(connection, $"more than one external accounts found on changeProvider {0} and account {accountId}", DataLayer.MongoData.Config.LogLevelEnum.OptionMessage);
			}

			return externalAccounts;
		}
	}
}
