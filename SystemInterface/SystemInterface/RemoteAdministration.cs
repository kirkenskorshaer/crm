using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace SystemInterface
{
	public class RemoteAdministration
	{
		private Collection<PSObject> ExecutePowershell(string ip, string username, string password, Action<PowerShell> powershellAction)
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

					powershellAction(powershell);

					Collection<PSObject> returnValue = powershell.Invoke();

					return returnValue;
				}
			}
		}

		public void CreateFile(string ip, string username, string password, string path, string value)
		{
			Action<PowerShell> powerShellAction = (powershell) =>
			{
				powershell.AddCommand("Set-Content");
				powershell.AddParameter("Path", path);
				powershell.AddParameter("Value", value);
			};

			ExecutePowershell(ip, username, password, powerShellAction);
		}

		public bool ServiceExists(string ip, string username, string password, string serviceName)
		{
			Action<PowerShell> powerShellAction = (powershell) =>
			{
				powershell.AddCommand("Get-Service");
				powershell.AddParameter("Name", serviceName);
			};

			Collection<PSObject> psObjects = ExecutePowershell(ip, username, password, powerShellAction);

			return psObjects.Count == 1;
		}

		public void CopyFile(string ip, string username, string password, string sourcePath, string targetPath)
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
					string powershellScript = "";
					powershellScript += Environment.NewLine + "$append = [System.io.FileMode]::Append";

					powershellScript += Environment.NewLine + "[System.io.FileStream] $targetStream = New-Object -TypeName 'System.io.FileStream' -ArgumentList(\"" + targetPath + "\", $append);";

					FileStream sourceStream = File.OpenRead(sourcePath);
					int bufferSize = 1024;
					byte[] readBuffer = new byte[bufferSize];
					int readBytes;
					while ((readBytes = sourceStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
					{
						string binaryString = readBuffer.ToList().Aggregate("", (summed, byteValue) => (string.IsNullOrWhiteSpace(summed) ? "" : summed + ",") + byteValue.ToString());
						powershellScript += Environment.NewLine + "[byte[]] $readBuffer = " + binaryString;
						powershellScript += Environment.NewLine + "$targetStream.Write($readBuffer, 0, " + readBytes + ");";
					}

					sourceStream.Flush();
					sourceStream.Dispose();

					powershellScript += Environment.NewLine + "$targetStream.Flush();";
					powershellScript += Environment.NewLine + "$targetStream.Dispose()";

					powershell.AddScript(powershellScript);

					powershell.Invoke();
				}
			}
		}
	}
}
