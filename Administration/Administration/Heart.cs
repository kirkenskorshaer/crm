using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administration
{
	public class Heart
	{
		private bool _run = true;

		public void Run()
		{
			while (_run)
			{
				try
				{
					HeartBeat();
				}
				catch
				{
					// ignored
				}
			}
		}

		public void HeartBeat()
		{
			_run = false;
		}
	}
}
