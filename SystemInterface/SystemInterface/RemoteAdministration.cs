using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace SystemInterface
{
	public class RemoteAdministration
	{
		public void CreateFile(string ip, string username, string password, string path, string value)
		{
			string shell = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

			using (SecureString passing = new SecureString())
			{
				foreach (char passwordCharacter in password)
				{
					passing.AppendChar(passwordCharacter);
				}

				passing.MakeReadOnly();

				PSCredential cred = new PSCredential(username, passing);

				WSManConnectionInfo connectionInfo = new WSManConnectionInfo(true, ip, 5986, "/wsman", shell, cred);

				connectionInfo.OperationTimeout = 4 * 60 * 1000; // 4 minutes.
				connectionInfo.OpenTimeout = 1 * 60 * 1000;
				connectionInfo.SkipCACheck = true;

				using (Runspace runSpace = RunspaceFactory.CreateRunspace(connectionInfo))
				{
					runSpace.Open();

					PowerShell powershell = PowerShell.Create();
					powershell.Runspace = runSpace;

					powershell.AddCommand("Set-Content");
					powershell.AddParameter("Path", path);
					powershell.AddParameter("Value", value);

					powershell.Invoke();
				}
			}
		}
	}
}
