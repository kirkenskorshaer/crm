using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public interface IDynamicsCrmConnection
	{
		IOrganizationService Service { get; }
	}
}
