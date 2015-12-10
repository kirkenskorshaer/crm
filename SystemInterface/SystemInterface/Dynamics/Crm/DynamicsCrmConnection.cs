using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Client;

namespace SystemInterface.Dynamics.Crm
{
	public class DynamicsCrmConnection
	{
		private static DynamicsCrmConnection _connection;
		public OrganizationService Service;
		public OrganizationServiceContext Context;

		private DynamicsCrmConnection(string url, string username, string password)
		{
			string connectionString = $"Url={url}; Username={username}; Password={password};";
			CrmConnection crmConnection = CrmConnection.Parse(connectionString);

			Service = new OrganizationService(crmConnection);
			Context = new OrganizationServiceContext(Service);

			Context.MergeOption = MergeOption.NoTracking;
		}

		public static DynamicsCrmConnection GetConnection(string url, string username, string password)
		{
			if (_connection == null)
			{
				_connection = new DynamicsCrmConnection(url, username, password);
			}

			return _connection;
		}
	}
}
