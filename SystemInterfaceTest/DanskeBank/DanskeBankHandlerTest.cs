using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using SystemInterface;
using SystemInterface.DanskeBank;

namespace SystemInterfaceTest.DanskeBank
{
	[TestFixture]
	public class DanskeBankHandlerTest
	{
		[Test]
		public void GetDownloadFileApplicationRequestGeneratesValidXml()
		{
			string customerUserId = "customerId";
			X509Certificate2 certificate = GetX509Certificate2();
			DanskeBankHandler danskeBankHandler = new DanskeBankHandler(customerUserId, certificate);

			XDocument documentXml = danskeBankHandler.GetDownloadFileApplicationRequestXml("fileReference_test", "test System");

			string xmlString = documentXml.ToString();

			Console.Out.WriteLine(xmlString);
		}

		[Test]
		public void DownloadFileApplicationRequestCanEncryptData()
		{
			string customerUserId = "customerId";
			X509Certificate2 certificate = GetX509Certificate2();
			DanskeBankHandler danskeBankHandler = new DanskeBankHandler(customerUserId, certificate);

			XDocument documentXml = danskeBankHandler.GetDownloadFileApplicationRequestXml("fileReference_test", "test System");

			XDocument encryptedXml = danskeBankHandler.EncryptApplicationRequest(certificate, documentXml);

			XDocument decryptedXml = danskeBankHandler.DecryptApplicationRequest(certificate, encryptedXml);

			//Console.Out.WriteLine(encryptedXml);
			//Console.Out.WriteLine(decryptedXml);

			Assert.AreEqual(documentXml.ToString(), decryptedXml.ToString());
		}

		[Test]
		public void DownloadFile()
		{
			string customerUserId = "customerId";
			X509Certificate2 certificate = GetX509Certificate2();

			InterfaceHelper initializer = InterfaceHelper.GetInstance();
			initializer.AllowThumbprint("e4bc48303644882ce5c6b39434d553e951de14f8");

			System.Diagnostics.Trace.AutoFlush = true;

			TraceSource mySource = new TraceSource("System.ServiceModel", SourceLevels.All);

			//mySource.Switch = new SourceSwitch("sourceSwitch", "Error");
			//mySource.Listeners.Remove("Default");
			//mySource.Listeners.
			//TextWriterTraceListener textListener = new TextWriterTraceListener("myListener.log");
			ConsoleTraceListener console = new ConsoleTraceListener(false);
			console.Filter = new EventTypeFilter(SourceLevels.All);
			console.Name = "console";
			//textListener.Filter = new EventTypeFilter(SourceLevels.Error);
			mySource.Listeners.Add(console);
			//mySource.Listeners.Add(textListener);

			// Allow the trace source to send messages to 
			// listeners for all event types. Currently only 
			// error messages or higher go to the listeners.
			// Messages must get past the source switch to 
			// get to the listeners, regardless of the settings 
			// for the listeners.
			mySource.Switch.Level = SourceLevels.All;


			DanskeBankHandler danskeBankHandler = new DanskeBankHandler(customerUserId, certificate);

			mySource.TraceEvent(TraceEventType.Error, 1, "Error message.");
			// Set the filter settings for the 
			// console trace listener.
			mySource.Listeners["console"].Filter = new EventTypeFilter(SourceLevels.Critical);
			//Activity2();

			// Change the filter settings for the console trace listener.
			mySource.Listeners["console"].Filter = new EventTypeFilter(SourceLevels.Information);
			//Activity3();


			//Trace.
			mySource.Close();

			//System.Diagnostics.TraceSource traceSource = new System.Diagnostics.TraceSource()
			//System.Diagnostics.lis
			/*
			

<system.diagnostics>
    <trace autoflush="true" />
    <sources>
            <source name="System.ServiceModel" 
                    switchValue="Information, ActivityTracing"
                    propagateActivity="true">
            <listeners>
               <add name="sdt" 
                   type="System.Diagnostics.XmlWriterTraceListener" 
                   initializeData= "SdrConfigExample.e2e" />
            </listeners>
         </source>
    </sources>
</system.diagnostics>

			*/


			
			XDocument documentXml = danskeBankHandler.DownloadIso20022("fileReference_test", "test System");

			string xmlString = documentXml.ToString();

			Console.Out.WriteLine(xmlString);
			
		}

		private X509Certificate2 GetX509Certificate2()
		{
			const string publicCert = @"MIIBrzCCARigAwIBAgIQEkeKoXKDFEuzql5XQnkY9zANBgkqhkiG9w0BAQUFADAYMRYwFAYDVQQDEw1DZXJ0QXV0aG9yaXR5MB4XDTEzMDQxOTIwMDAwOFoXDTM5MTIzMTIzNTk1OVowFjEUMBIGA1UEAxMLc2VydmVyMS5jb20wgZ0wDQYJKoZIhvcNAQEBBQADgYsAMIGHAoGBAIEmC1/io4RNMPCpYanPakMYZGboMCrN6kqoIuSI1n0ufzCbwRkpUjJplsvRH9ijIHMKw8UVs0i0Ihn9EnTCxHgM7icB69u9EaikVBtfSGl4qUy5c5TZfbN0P3MmBq4YXo/vXvCDDVklsMFem57COAaVvAhv+oGv5oiqEJMXt+j3AgERMA0GCSqGSIb3DQEBBQUAA4GBAICWZ9/2zkiC1uAend3s2w0pGQSz4RQeh9+WiT4n3HMwBGjDUxAx73fhaKADMZTHuHT6+6Q4agnTnoSaU+Fet1syVVxjLeDHOb0i7o/IDUWoEvYATi8gCtcV20KxsQVLEc5jkkajzUc0eyg050KZaLzV+EkCKBafNoVFHoMCbm3n";
			const string privateCert = @"<RSAKeyValue><Modulus>gSYLX+KjhE0w8Klhqc9qQxhkZugwKs3qSqgi5IjWfS5/MJvBGSlSMmmWy9Ef2KMgcwrDxRWzSLQiGf0SdMLEeAzuJwHr270RqKRUG19IaXipTLlzlNl9s3Q/cyYGrhhej+9e8IMNWSWwwV6bnsI4BpW8CG/6ga/miKoQkxe36Pc=</Modulus><Exponent>EQ==</Exponent><P>mmRPs28vh0mOsnQOder5fsxKsuGhBkz+mApKTNQZkkn7Ak3CWKaFzCI3ZBZUpTJag841LL45uM2NvesFn/T25Q==</P><Q>1iTLW2zHVIYi+A6Pb0UarMaBvOnH0CTP7xMEtLZD5MFYtqG+u45mtFj1w49ez7n5tq8WyOs90Jq1qhnKGJ0mqw==</Q><DP>JFPWhJKhxXq4Kf0wlDdJw3tc3sutauTwnD6oEhPJyBFoPMcAjVRbt4+UkAVBF8+c07gMgv+VHGyZ0lVqvDmjgQ==</DP><DQ>lykIBEzI8F6vRa/sxwOaW9dqo3fYVrCSxuA/jp7Gg1tNrhfR7c3uJPOATc6dR1YZriE9QofvZhLaljBSa7o5aQ==</DQ><InverseQ>KrrKkN4IKqqhrcpZbYIWH4rWoCcnfTI5jxMfUDKUac+UFGNxHCUGLe1x+rwz4HcOA7bKVECyGe6C9xeiN3XKuQ==</InverseQ><D>Fsp6elUr6iu9V6Vrlm/lk16oTmU1rTNllLRCZJCeUlN/22bHuSVo27hHyZ1f+Q26bqeL9Zpq7rZgXvBsqzFt9tBOESrkr+uEHIZwQ1HIDw2ajxwOnlrj+zjn6EKshrMOsEXXbgSAi6SvGifRC2f+TKawt9lZmGElV4QgMYlC56k=</D></RSAKeyValue>";

			X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(publicCert));

			var crypto = new RSACryptoServiceProvider();

			crypto.FromXmlString(privateCert);

			certificate.PrivateKey = crypto;

			//export a private key
			var exportedPrivate = certificate.PrivateKey.ToXmlString(true);
			var exportedPublic = Convert.ToBase64String(certificate.RawData);

			Assert.AreEqual(publicCert, exportedPublic);
			Assert.AreEqual(privateCert, exportedPrivate);

			return certificate;
		}
	}
}
