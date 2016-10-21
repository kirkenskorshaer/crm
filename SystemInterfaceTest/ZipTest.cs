using System.Collections.Generic;
using System.IO;
using System.Linq;
using SystemInterface;
using NUnit.Framework;
using System;
using System.Text;

namespace SystemInterfaceTest
{
	[TestFixture]
	public class ZipTest
	{
		private const string basePath = "C:/test/compress";

		private void DeleteDirectoryAndFiles(string path)
		{
			List<string> directories = Directory.GetDirectories(path).ToList();
			directories.ForEach(directory => DeleteDirectoryAndFiles(directory));

			List<string> files = Directory.GetFiles(path).ToList();
			files.ForEach(filename => File.Delete(filename));
			Directory.Delete(path);
		}

		[SetUp]
		public void Setup()
		{
			if (Directory.Exists(basePath))
			{
				DeleteDirectoryAndFiles(basePath);
			}

			Directory.CreateDirectory(basePath);
			File.WriteAllText(basePath + "/test1.txt", "testContentTop1");
			File.WriteAllText(basePath + "/test2.txt", "testContentTop2");

			string subDirectory = basePath + "/dir";

			Directory.CreateDirectory(subDirectory);
			File.WriteAllText(subDirectory + "/test.txt", "testContentSubdirectory");
		}

		[TestCase(basePath + "/test1.txt", basePath + "/result/test1.txt")]
		[TestCase(basePath + "/test2.txt", basePath + "/result/test2.txt")]
		[TestCase(basePath + "/dir/test.txt", basePath + "/result/dir/test.txt")]
		public void CompressTest(string pathBefore, string pathAfter)
		{
			string archiveName = basePath + "/zipped.zip";
			string resultFolder = basePath + "/result";

			Zip zip = new Zip();
			zip.CompressFolder(basePath, archiveName);

			zip.DecompressFolder(archiveName, resultFolder);

			string before = File.ReadAllText(pathBefore);
			string after = File.ReadAllText(pathAfter);

			Assert.AreEqual(before, after);
		}

		[Test]
		public void AStringCanBeCompressed()
		{
			string uncompressed = "qwertyuiopåasdfghjklæø'¨1234567890+´`<zxcvbnm,.->;:_*^!`\"#¤%&/()=?";
			Zip zip = new Zip();

			byte[] compressed = zip.CompressString(uncompressed);

			Assert.AreNotEqual(compressed, uncompressed);

			string decompressed = zip.DecompressString(compressed);

			Assert.AreEqual(uncompressed, decompressed);

			Console.Out.WriteLine(uncompressed);
			StringBuilder reportStringBuilder = new StringBuilder();
			compressed.ToList().ForEach(compressByte => reportStringBuilder.Append(compressByte.ToString() + ","));
			Console.Out.WriteLine(reportStringBuilder.ToString());
			Console.Out.WriteLine(decompressed);
		}
	}
}
