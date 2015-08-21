using Ionic.Zip;

namespace SystemInterface
{
	public class Zip
	{
		public void CompressFolder(string sourceFolderPath, string archiveFolderName)
		{
			using (ZipFile zipFile = new ZipFile())
			{
				zipFile.AddDirectory(sourceFolderPath);
				zipFile.Save(archiveFolderName);
			}
		}

		public void DecompressFolder(string archiveFolderName, string targetFolderName)
		{
			using (ZipFile zipFile = ZipFile.Read(archiveFolderName))
			{
				zipFile.ExtractAll(targetFolderName);
			}
		}
	}
}
