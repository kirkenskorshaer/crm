using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace SystemInterfaceTest.Dynamics.Crm
{
	[TestFixture]
	public class AnnotationTest : TestBase
	{
		[Test]
		public void AnnotationCanBeCreated()
		{
			Contact crmContact = CreateTestContact();
			crmContact.Insert();

			Annotation annotationCreated = new Annotation(_dynamicsCrmConnection);
			annotationCreated.notetext = $"note {Guid.NewGuid()}";
			annotationCreated.Insert();

			crmContact.SynchronizeAnnotations(new List<Annotation>() { annotationCreated });

			List<Annotation> annotations = crmContact.GetAnnotations();

			crmContact.Delete();

			Assert.AreEqual(annotationCreated.notetext, annotations.Single().notetext);
		}
	}
}
