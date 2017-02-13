using System;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Collections.Generic;

namespace DynamicsCrmPluginTest.Mock
{
	public class TracingServiceTest : ITracingService
	{
		private const string _spacer = "     ";

		public void Trace(string format, params object[] args)
		{
			try
			{
				Console.Out.WriteLine(string.Format(_spacer + format, args));
			}
			catch (Exception)
			{
				List<string> argsStringList = args.Select(arg => arg.ToString()).ToList();

				string argsString = string.Empty;
				if (argsStringList.Any())
				{
					argsString = argsStringList.Aggregate((returned, arg) => returned + "," + arg);
				}

				Console.Out.WriteLine($"{_spacer}{format} - {argsString}");
			}
		}
	}
}
