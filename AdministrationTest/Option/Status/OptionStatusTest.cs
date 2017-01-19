using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministrationTest.Option.Status
{
	[TestFixture]
	public class OptionStatusTest// : TestBase
	{
		[Test]
		public void test()
		{
			System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
			//long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;

			Console.Out.WriteLine($"Id: {currentProcess.Id}");
			Console.Out.WriteLine($"VirtualMemorySize64: {currentProcess.VirtualMemorySize64 / (1024 * 1024)} MB");
		}
	}
}
