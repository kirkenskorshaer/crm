using System.ComponentModel;
using System.ServiceProcess;
using Administration;

namespace ServiceRunner
{
	public partial class CrmService : ServiceBase
	{
		private BackgroundWorker _backgroundWorker;
		private Heart _heart;

		public CrmService()
		{
			InitializeComponent();
		}

		private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			_heart.Run();
		}

		protected override void OnStart(string[] args)
		{
			_backgroundWorker = new BackgroundWorker();
			_heart = new Heart();

			_backgroundWorker.DoWork += _backgroundWorker_DoWork;

			_backgroundWorker.RunWorkerAsync();
		}

		protected override void OnStop()
		{
			_heart.Stop();
		}
	}
}
