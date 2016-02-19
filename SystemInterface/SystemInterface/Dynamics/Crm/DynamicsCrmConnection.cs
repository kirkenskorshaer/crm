using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Client;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class DynamicsCrmConnection
	{
		private static List<DynamicsCrmConnection> _connections = new List<DynamicsCrmConnection>();
		public OrganizationService Service;
		public OrganizationServiceContext Context;

		private string url;
		private string username;
		private string password;

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
			DynamicsCrmConnection connection = _connections.SingleOrDefault(lConnection =>
				lConnection.url == url &&
				lConnection.username == username &&
				lConnection.password == password);

			if (connection == null)
			{
				connection = new DynamicsCrmConnection(url, username, password);
				_connections.Add(connection);
			}

			return connection;
		}
	}
}
