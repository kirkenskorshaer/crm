using System.ComponentModel;
using System.ServiceProcess;
using Administration;
using System.Threading;

namespace ServiceRunner
{
	public partial class CrmService : ServiceBase
	{
		private Heart _heart;
		private Thread _mainThread;

		public CrmService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			_heart = new Heart();

			_mainThread = new Thread(new ThreadStart(_heart.Run));
			_mainThread.Start();
		}

		protected override void OnStop()
		{
			_heart.Stop();
		}
	}
}
