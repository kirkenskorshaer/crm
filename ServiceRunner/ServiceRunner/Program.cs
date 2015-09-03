using System.ServiceProcess;

namespace ServiceRunner
{
	static class Program
	{
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new CrmService()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
