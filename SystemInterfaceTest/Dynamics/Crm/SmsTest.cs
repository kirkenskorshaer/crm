using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterface.Dynamics.Crm;
using Utilities;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class SmsTest : TestBase
	{
		[Test]
		public void GetTemplate()
		{
			SmsTemplate template = SmsTemplate.GetWaitingTemplate(_dynamicsCrmConnection);

			Console.Out.WriteLine(template.new_text);
			Console.Out.WriteLine(template.Id);
			Console.Out.WriteLine(template.new_fetchxml);

			Sms sms = Sms.GetWaitingSmsOnTemplate(_dynamicsCrmConnection, template.Id).Single();

			//Console.Out.WriteLine(sms.mobilephone);
			//Console.Out.WriteLine(sms.Id);
			//Console.Out.WriteLine(sms.toid);

			IDictionary<string, object> fields = template.GetFields(sms.toid.Value);

			/*
			foreach (KeyValuePair<string, object> content in fields)
			{
				Console.Out.WriteLine($"{content.Key} : {content.Value.ToString()}");
			}
			*/

			TextMerger textMerger = new TextMerger(template.new_text);
			string output = textMerger.GetMerged(fields, "yyyy-MM-dd");

			Console.Out.WriteLine(output);
		}
	}
}
