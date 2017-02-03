using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.Collections.Generic;
using System.Linq;

namespace SystemInterface.Dynamics.Crm
{
	public class DynamicsCrmConnection : IDynamicsCrmConnection
	{
		private static List<DynamicsCrmConnection> _connections = new List<DynamicsCrmConnection>();
		public IOrganizationService Service { get; }

		private string url;
		private string username;
		private string password;

		private DynamicsCrmConnection(string url, string username, string password)
		{
			string connectionString = $"AuthType=IFD;Domain=KAD;Url={url}; Username={username}; Password={password}";

			CrmServiceClient client = new CrmServiceClient(connectionString);
			Service = client.OrganizationServiceProxy;
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
