using System.ServiceProcess;

namespace ServiceRunner
{
	public partial class CrmService : ServiceBase
	{
		public CrmService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
		}

		protected override void OnStop()
		{
		}
	}
}
