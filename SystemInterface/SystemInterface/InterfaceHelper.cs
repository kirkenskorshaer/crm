using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SystemInterface
{
	public class InterfaceHelper
	{
		private List<string> _validCertificateThumbprints = new List<string>();
		private static InterfaceHelper _instance;

		private InterfaceHelper()
		{
		}

		public static InterfaceHelper GetInstance()
		{
			if (_instance == null)
			{
				_instance = new InterfaceHelper();
			}

			return _instance;
		}

		public void AllowThumbprint(string thumbprint)
		{
			AllowThumbprint(thumbprint, true);
		}

		public void AllowThumbprints(List<string> thumbprints)
		{
			thumbprints.ForEach(thumbprint => AllowThumbprint(thumbprint, false));
			InitializeCertificates();
		}

		private void AllowThumbprint(string thumbprint, bool initializeCertificates)
		{
			if (_validCertificateThumbprints.Contains(thumbprint))
			{
				return;
			}

			_validCertificateThumbprints.Add(thumbprint);

			if (initializeCertificates)
			{
				InitializeCertificates();
			}
		}

		private void InitializeCertificates()
		{
			System.Net.ServicePointManager.ServerCertificateValidationCallback = TrustValidation;
		}

		private bool TrustValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}

			X509Certificate2 certificate2 = new X509Certificate2(certificate);
			string thumbprint = certificate2.Thumbprint.TrimStart('‎').ToLower();

			if (_validCertificateThumbprints.Any(lThumbprint => lThumbprint.TrimStart('‎').ToLower() == thumbprint))
			{
				return true;
			}

			return false;
		}
	}
}
