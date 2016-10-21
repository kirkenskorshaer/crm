using Ionic.Zip;
using System.IO;
using System.IO.Compression;
using System.Text;

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

		public byte[] CompressString(string uncompressed)
		{
			byte[] output;

			using (MemoryStream memoryStreamInput = new MemoryStream())
			{
				StreamWriter writer = new StreamWriter(memoryStreamInput);
				writer.Write(uncompressed);
				writer.Flush();

				memoryStreamInput.Position = 0;

				using (MemoryStream memoryStreamOutput = new MemoryStream())
				{
					using (GZipStream gZipStream = new GZipStream(memoryStreamOutput, CompressionMode.Compress))
					{
						memoryStreamInput.CopyTo(gZipStream);
					}
					output = memoryStreamOutput.ToArray();
				}
			}

			return output;
		}

		public string DecompressString(byte[] compressed)
		{
			string output;

			using (MemoryStream memoryStreamInput = new MemoryStream(compressed))
			{
				using (GZipStream gZipStream = new GZipStream(memoryStreamInput, CompressionMode.Decompress))
				{
					using (MemoryStream memoryStreamOutput = new MemoryStream())
					{
						gZipStream.CopyTo(memoryStreamOutput);
						output = Encoding.UTF8.GetString(memoryStreamOutput.ToArray());
					}

				}
			}

			return output;
		}
	}
}
