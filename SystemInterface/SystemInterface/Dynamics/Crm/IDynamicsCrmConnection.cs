using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace SystemInterface.Dynamics.Crm
{
	public interface IDynamicsCrmConnection
	{
		IOrganizationService Service { get; }
		OrganizationServiceContext Context { get; }
	}
}
